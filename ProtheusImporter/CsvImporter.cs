using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;

namespace ProtheusImporter.Core;

/// <summary>
/// Engine generico de importacao CSV -> tabela EF, com UPSERT por chave natural.
///
/// Fluxo:
///   1. Le CSV (detecta header, valida colunas obrigatorias).
///   2. Converte cada linha em entidade via mapper.
///   3. Agrupa por chave e identifica repetidos (todas as linhas onde a chave apareceu).
///   4. Carrega em memoria as chaves ja existentes no banco (1 query).
///   5. Classifica em Novos / Atualizacoes.
///   6. Se modo = ResetarEInserir: deleta tudo antes de inserir.
///   7. Salva em batches (BatchSize) com ChangeTracker.Clear() entre eles.
///
/// Observacao: o Id do registro existente e PRESERVADO em atualizacoes.
/// </summary>
public sealed class CsvImporter<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    private readonly AppDbContext _db;
    private readonly ImportOptions _opts;
    private readonly Func<CsvRow, TEntity?> _mapper;
    private readonly Func<TEntity, TKey> _chave;
    private readonly Action<TEntity, TEntity> _atualizar;
    private readonly IEqualityComparer<TKey> _keyComparer;

    /// <param name="db">Contexto EF do ARJ.</param>
    /// <param name="opts">Opcoes de importacao.</param>
    /// <param name="mapper">Converte uma linha do CSV em entidade. Retornar null = rejeita linha.</param>
    /// <param name="chave">Extrai a chave natural da entidade (usada pro UPSERT).</param>
    /// <param name="atualizar">Copia dados da entidade nova (CSV) pra existente (banco). NAO altera Id.</param>
    /// <param name="keyComparer">Comparer de chave. Default: EqualityComparer&lt;TKey&gt;.Default.</param>
    public CsvImporter(
        AppDbContext db,
        ImportOptions opts,
        Func<CsvRow, TEntity?> mapper,
        Func<TEntity, TKey> chave,
        Action<TEntity, TEntity> atualizar,
        IEqualityComparer<TKey>? keyComparer = null)
    {
        _db = db;
        _opts = opts;
        _mapper = mapper;
        _chave = chave;
        _atualizar = atualizar;
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// Executa a analise do CSV sem gravar no banco. Retorna o que seria feito.
    /// Usado pra mostrar relatorio de preview.
    /// </summary>
    public PreviewResult<TEntity, TKey> Analisar()
    {
        var sw = Stopwatch.StartNew();
        var resultado = new PreviewResult<TEntity, TKey>();

        var (_, linhas) = CsvReader.Ler(_opts);
        resultado.LinhasLidas = linhas.Count;

        // Passo 1: converte todas as linhas em entidades e agrupa por chave, coletando
        // o numero de CADA linha em que a chave aparece. Isso permite depois listar no log
        // "chave X apareceu nas linhas 300, 4521, 9832".
        var porChave = new Dictionary<TKey, List<(int Linha, TEntity Entidade)>>(_keyComparer);
        int linhaIdx = 0;

        foreach (var row in linhas)
        {
            linhaIdx++;
            TEntity? entidade;
            try
            {
                entidade = _mapper(row);
            }
            catch (Exception ex)
            {
                resultado.Rejeitados++;
                resultado.Mensagens.Add($"Linha {linhaIdx}: {ex.Message}");
                continue;
            }

            if (entidade is null)
            {
                resultado.Rejeitados++;
                continue;
            }

            var k = _chave(entidade);
            if (!porChave.TryGetValue(k, out var lista))
            {
                lista = new List<(int, TEntity)>();
                porChave[k] = lista;
            }
            lista.Add((linhaIdx, entidade));
        }

        // Passo 2: pra cada chave com mais de uma linha, registra o repetido
        // (com todas as linhas). Entidade final usada = ULTIMA ocorrencia.
        var finalPorChave = new Dictionary<TKey, TEntity>(_keyComparer);
        foreach (var (chave, ocorrencias) in porChave)
        {
            if (ocorrencias.Count > 1)
            {
                resultado.RepetidosTotal += ocorrencias.Count - 1;
                var item = new ItemRepetido { Chave = chave.ToString() ?? "" };
                foreach (var (linha, _) in ocorrencias) item.Linhas.Add(linha);
                resultado.Repetidos.Add(item);
            }

            finalPorChave[chave] = ocorrencias[^1].Entidade;
        }

        // Passo 3: carrega chaves ja existentes no banco em 1 query.
        var existentes = _db.Set<TEntity>()
            .AsNoTracking()
            .ToList()
            .ToDictionary(e => _chave(e), e => e, _keyComparer);

        foreach (var (chave, novaEntidade) in finalPorChave)
        {
            if (existentes.TryGetValue(chave, out var atual))
            {
                resultado.AtualizacoesPrevistas[chave] = (atual, novaEntidade);
            }
            else
            {
                resultado.InsercoesPrevistas[chave] = novaEntidade;
            }
        }

        resultado.TotalExistentes = existentes.Count;
        resultado.Duracao = sw.Elapsed;
        return resultado;
    }

    /// <summary>
    /// Aplica uma previa ja analisada ao banco, em batches.
    /// Se modo = ResetarEInserir, deleta TUDO antes e insere todo mundo como novo.
    /// </summary>
    public ImportResult Aplicar(PreviewResult<TEntity, TKey> preview, Action<int, int>? onProgress = null)
    {
        var sw = Stopwatch.StartNew();
        var resultado = new ImportResult
        {
            LinhasLidas = preview.LinhasLidas,
            Rejeitados = preview.Rejeitados,
            RepetidosTotal = preview.RepetidosTotal
        };
        resultado.Mensagens.AddRange(preview.Mensagens);
        resultado.Repetidos.AddRange(preview.Repetidos);

        using var tx = _db.Database.BeginTransaction();
        try
        {
            if (_opts.Modo == ModoImportacao.ResetarEInserir)
            {
                _db.Set<TEntity>().ExecuteDelete();
                _db.ChangeTracker.Clear();

                var todos = preview.InsercoesPrevistas.Values
                    .Concat(preview.AtualizacoesPrevistas.Values.Select(v => v.Novo))
                    .ToList();

                resultado.Novos = todos.Count;
                resultado.Atualizados = 0;

                SalvarEmBatches(todos, onProgress, totalEsperado: todos.Count);
            }
            else
            {
                // UPSERT incremental.
                var novos = preview.InsercoesPrevistas.Values.ToList();
                resultado.Novos = novos.Count;

                var atualizacoes = preview.AtualizacoesPrevistas.Values.ToList();
                resultado.Atualizados = atualizacoes.Count;

                int totalEsperado = novos.Count + atualizacoes.Count;
                SalvarEmBatches(novos, onProgress, totalEsperado);
                AplicarAtualizacoesEmBatches(atualizacoes, onProgress, totalEsperado, jaProcessados: novos.Count);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        resultado.Duracao = sw.Elapsed;
        return resultado;
    }

    private void SalvarEmBatches(List<TEntity> entidades, Action<int, int>? onProgress, int totalEsperado)
    {
        int processados = 0;
        for (int i = 0; i < entidades.Count; i += _opts.BatchSize)
        {
            var batch = entidades.Skip(i).Take(_opts.BatchSize).ToList();
            _db.Set<TEntity>().AddRange(batch);
            _db.SaveChanges();
            _db.ChangeTracker.Clear();

            processados += batch.Count;
            onProgress?.Invoke(processados, totalEsperado);
        }
    }

    private void AplicarAtualizacoesEmBatches(
        List<(TEntity Atual, TEntity Novo)> pares,
        Action<int, int>? onProgress,
        int totalEsperado,
        int jaProcessados)
    {
        int processados = jaProcessados;
        for (int i = 0; i < pares.Count; i += _opts.BatchSize)
        {
            var batch = pares.Skip(i).Take(_opts.BatchSize).ToList();

            foreach (var (atual, novo) in batch)
            {
                _db.Attach(atual);
                _atualizar(atual, novo);
            }

            _db.SaveChanges();
            _db.ChangeTracker.Clear();

            processados += batch.Count;
            onProgress?.Invoke(processados, totalEsperado);
        }
    }
}

/// <summary>
/// Resultado da fase de analise (preview) — antes de qualquer gravacao.
/// </summary>
public sealed class PreviewResult<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    public int LinhasLidas { get; set; }
    public int RepetidosTotal { get; set; }
    public int Rejeitados { get; set; }
    public int TotalExistentes { get; set; }
    public TimeSpan Duracao { get; set; }

    /// <summary>SG1: total de codigos-pai no CSV (apos filtro de Rev. Final).</summary>
    public int PaisComRevisao { get; set; }

    /// <summary>SG1: pais que tinham mais de uma Rev. Final no CSV.</summary>
    public int PaisFiltrados { get; set; }

    /// <summary>SG1: linhas descartadas por estar em Rev. Final antiga.</summary>
    public int LinhasDescartadasRevAntiga { get; set; }

    public Dictionary<TKey, TEntity> InsercoesPrevistas { get; } = new();
    public Dictionary<TKey, (TEntity Atual, TEntity Novo)> AtualizacoesPrevistas { get; } = new();

    public List<string> Mensagens { get; } = new();
    public List<ItemRepetido> Repetidos { get; } = new();
    public List<RejeicaoSG1> RejeicoesSG1 { get; } = new();

    public int TotalAGravar => InsercoesPrevistas.Count + AtualizacoesPrevistas.Count;
}
