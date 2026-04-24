using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using ProtheusImporter.Core;

namespace ProtheusImporter.Importers;

/// <summary>
/// Importador Protheus SG1 (estrutura/BOM) para Engenharia_EstruturasProdutos do ARJ.
///
/// Colunas obrigatorias no CSV: Codigo, Ordem Item, Sequencia, Quantidade, Componente
/// Colunas opcionais no CSV:    Descricao (descartada — nao usamos)
///
/// Regras de dedup (ordem):
///   (1) Mesmo pai + mesmo filho + MESMA Sequencia = repetido real.
///       Log reporta nas "REPETIDOS NO CSV". Mantem apenas a ultima ocorrencia.
///   (2) Mesmo pai + mesmo filho + Sequencias DIFERENTES = caso legitimo do Protheus.
///       Como o ARJ ainda nao tem Sequencia na Estrutura, o comportamento depende da
///       flag .ini SomarSequenciasRepetidas:
///         true  = soma as Quantidades e grava 1 linha consolidada
///         false = mantem so a ultima ocorrencia (pra quando ARJ tiver Sequencia)
///       Esse caso NAO entra como repetido no log — e consolidacao proposital.
///
/// Rejeicoes (linhas descartadas, nao importadas):
///   - Codigo pai ou componente vazio
///   - Pai ou filho nao encontrado em Produtos (SB1)
///   - Pai == Filho (auto-referencia)
///   Todas viram RejeicaoSG1 com status encontrado/nao encontrado pra cada lado.
/// </summary>
public static class ImportadorSG1
{
    public const string COL_CODIGO_PAI = "Código";
    public const string COL_ORDEM_ITEM = "Ordem Item";
    public const string COL_SEQUENCIA = "Sequência";
    public const string COL_QUANTIDADE = "Quantidade";
    public const string COL_COMPONENTE = "Componente";
    public const string COL_REV_FINAL = "Rev. Final";

    /// <summary>
    /// Registro intermediario de uma linha valida do CSV. Mantido pra consolidacao.
    /// </summary>
    private sealed class LinhaSG1
    {
        public int Linha { get; set; }
        public int PaiId { get; set; }
        public int FilhoId { get; set; }
        public string CodigoPai { get; set; } = string.Empty;
        public string CodigoFilho { get; set; } = string.Empty;
        public string Sequencia { get; set; } = string.Empty;
        public int RevFinal { get; set; }
        public decimal Quantidade { get; set; }
        public int Posicao { get; set; }
    }

    /// <summary>
    /// Executa a fase de analise (preview) do CSV SG1.
    /// Nao grava nada. Retorna o que seria inserido/atualizado, os repetidos e as rejeicoes.
    ///
    /// Ordem de processamento:
    ///   Fase 1 — le o CSV e coleta linhas brutas (sem validar Produtos ainda).
    ///            So rejeita aqui casos triviais: codigo pai/filho vazios.
    ///   Fase 2 — agrupa por codigo pai, determina Rev. Final MAXIMA de cada pai,
    ///            descarta linhas de revisoes antigas (nao vao pro log de rejeicao,
    ///            so contam no resumo da revisao).
    ///   Fase 3 — so agora valida pai/filho em Produtos. Auto-referencia bloqueada.
    ///   Fase 4 — agrupa por (pai, filho, sequencia). Triade repetida = REPETIDO NO CSV.
    ///   Fase 5 — consolida por (pai, filho): soma Quantidades se flag ligada, senao
    ///            mantem ultima.
    /// </summary>
    public static PreviewResult<EstruturaProduto, (int PaiId, int FilhoId)> Analisar(
        AppDbContext db, ImportOptions opts)
    {
        var sw = Stopwatch.StartNew();
        var resultado = new PreviewResult<EstruturaProduto, (int, int)>();

        var lookupProdutos = db.Produtos
            .AsNoTracking()
            .Select(p => new { p.Id, p.Codigo })
            .ToDictionary(p => p.Codigo, p => p.Id, StringComparer.Ordinal);

        var (_, linhas) = CsvReader.Ler(opts);
        resultado.LinhasLidas = linhas.Count;

        // --- Fase 1: coleta linhas brutas (CodigoPai, CodigoFilho, RevFinal, etc.) ---
        var brutas = new List<LinhaSG1>();
        int linhaIdx = 0;

        foreach (var row in linhas)
        {
            linhaIdx++;

            var codigoPai = row.Get(COL_CODIGO_PAI);
            var codigoFilho = row.Get(COL_COMPONENTE);
            var sequencia = row.Get(COL_SEQUENCIA).Trim();
            var revFinal = ParseRevFinal(row.Get(COL_REV_FINAL));

            if (string.IsNullOrEmpty(codigoPai) || string.IsNullOrEmpty(codigoFilho))
            {
                resultado.Rejeitados++;
                resultado.Mensagens.Add($"Linha {linhaIdx}: codigo pai ou componente vazio. [pai='{codigoPai}', filho='{codigoFilho}']");
                continue;
            }

            brutas.Add(new LinhaSG1
            {
                Linha = linhaIdx,
                CodigoPai = codigoPai,
                CodigoFilho = codigoFilho,
                Sequencia = sequencia,
                RevFinal = revFinal,
                Quantidade = ParseQuantidade(row.Get(COL_QUANTIDADE)),
                Posicao = ParseOrdemItem(row.Get(COL_ORDEM_ITEM))
            });
        }

        // --- Fase 2: pra cada codigo pai, descobrir Rev. Final maxima ---
        //           Linhas de rev menor sao descartadas (nao viram rejeicao).
        var porPai = brutas.GroupBy(l => l.CodigoPai, StringComparer.Ordinal);
        var linhasFiltradas = new List<LinhaSG1>();

        foreach (var grupo in porPai)
        {
            resultado.PaisComRevisao++;
            var revMax = grupo.Max(l => l.RevFinal);
            var revs = grupo.Select(l => l.RevFinal).Distinct().Count();
            if (revs > 1) resultado.PaisFiltrados++;

            foreach (var linha in grupo)
            {
                if (linha.RevFinal == revMax)
                    linhasFiltradas.Add(linha);
                else
                    resultado.LinhasDescartadasRevAntiga++;
            }
        }

        // --- Fase 3: valida pai/filho em Produtos SOMENTE nas linhas da rev maxima ---
        var validas = new List<LinhaSG1>();
        foreach (var l in linhasFiltradas)
        {
            var paiExiste = lookupProdutos.TryGetValue(l.CodigoPai, out var paiId);
            var filhoExiste = lookupProdutos.TryGetValue(l.CodigoFilho, out var filhoId);

            if (!paiExiste || !filhoExiste)
            {
                resultado.Rejeitados++;
                resultado.RejeicoesSG1.Add(new RejeicaoSG1
                {
                    Linha = l.Linha,
                    CodigoPai = l.CodigoPai,
                    CodigoComponente = l.CodigoFilho,
                    PaiEncontrado = paiExiste,
                    ComponenteEncontrado = filhoExiste
                });
                continue;
            }

            if (paiId == filhoId)
            {
                resultado.Rejeitados++;
                resultado.Mensagens.Add($"Linha {l.Linha}: pai e filho sao o mesmo produto. [pai='{l.CodigoPai}', filho='{l.CodigoFilho}']");
                continue;
            }

            l.PaiId = paiId;
            l.FilhoId = filhoId;
            validas.Add(l);
        }

        // --- Fase 4: agrupa por triade (pai, filho, sequencia). Triade repetida = repetido real. ---
        var porTriade = new Dictionary<(int, int, string), List<LinhaSG1>>();
        foreach (var v in validas)
        {
            var tri = (v.PaiId, v.FilhoId, v.Sequencia);
            if (!porTriade.TryGetValue(tri, out var lista))
            {
                lista = new List<LinhaSG1>();
                porTriade[tri] = lista;
            }
            lista.Add(v);
        }

        var unicasPorTriade = new List<LinhaSG1>();
        foreach (var (_, ocorrencias) in porTriade)
        {
            if (ocorrencias.Count > 1)
            {
                resultado.RepetidosTotal += ocorrencias.Count - 1;
                var first = ocorrencias[0];
                var item = new ItemRepetido
                {
                    Chave = first.CodigoPai,
                    Detalhe = $"COMP.: {first.CodigoFilho} - SEQ.: {first.Sequencia}"
                };
                foreach (var oc in ocorrencias) item.Linhas.Add(oc.Linha);
                resultado.Repetidos.Add(item);
            }
            unicasPorTriade.Add(ocorrencias[^1]);
        }

        // --- Fase 5: consolida (pai, filho) — ARJ nao tem Sequencia ainda ---
        var finalPorPar = new Dictionary<(int, int), EstruturaProduto>();

        foreach (var v in unicasPorTriade)
        {
            var par = (v.PaiId, v.FilhoId);
            if (finalPorPar.TryGetValue(par, out var existente))
            {
                if (opts.SomarSequenciasRepetidas)
                    existente.Quantidade += v.Quantidade;
                else
                    existente.Quantidade = v.Quantidade;

                existente.Posicao = v.Posicao;
            }
            else
            {
                finalPorPar[par] = new EstruturaProduto
                {
                    ProdutoPaiId = v.PaiId,
                    ProdutoFilhoId = v.FilhoId,
                    Quantidade = v.Quantidade,
                    Posicao = v.Posicao,
                    Observacao = null
                };
            }
        }

        var existentesDb = db.EstruturasProdutos
            .AsNoTracking()
            .ToDictionary(e => (e.ProdutoPaiId, e.ProdutoFilhoId), e => e);

        foreach (var (chave, nova) in finalPorPar)
        {
            if (existentesDb.TryGetValue(chave, out var atual))
                resultado.AtualizacoesPrevistas[chave] = (atual, nova);
            else
                resultado.InsercoesPrevistas[chave] = nova;
        }

        resultado.TotalExistentes = existentesDb.Count;
        resultado.Duracao = sw.Elapsed;
        return resultado;
    }

    /// <summary>
    /// Aplica a previa ao banco em batches.
    /// </summary>
    public static ImportResult Aplicar(
        AppDbContext db,
        ImportOptions opts,
        PreviewResult<EstruturaProduto, (int PaiId, int FilhoId)> preview,
        Action<int, int>? onProgress = null)
    {
        var sw = Stopwatch.StartNew();
        var resultado = new ImportResult
        {
            LinhasLidas = preview.LinhasLidas,
            Rejeitados = preview.Rejeitados,
            RepetidosTotal = preview.RepetidosTotal,
            PaisComRevisao = preview.PaisComRevisao,
            PaisFiltrados = preview.PaisFiltrados,
            LinhasDescartadasRevAntiga = preview.LinhasDescartadasRevAntiga
        };
        resultado.Mensagens.AddRange(preview.Mensagens);
        resultado.Repetidos.AddRange(preview.Repetidos);
        resultado.RejeicoesSG1.AddRange(preview.RejeicoesSG1);

        using var tx = db.Database.BeginTransaction();
        try
        {
            if (opts.Modo == ModoImportacao.ResetarEInserir)
            {
                db.EstruturasProdutos.ExecuteDelete();
                db.ChangeTracker.Clear();

                var todos = preview.InsercoesPrevistas.Values
                    .Concat(preview.AtualizacoesPrevistas.Values.Select(v => v.Novo))
                    .ToList();

                resultado.Novos = todos.Count;
                InserirEmBatches(db, opts, todos, onProgress, totalEsperado: todos.Count);
            }
            else
            {
                var novos = preview.InsercoesPrevistas.Values.ToList();
                resultado.Novos = novos.Count;

                var atualizacoes = preview.AtualizacoesPrevistas.Values.ToList();
                resultado.Atualizados = atualizacoes.Count;

                int totalEsperado = novos.Count + atualizacoes.Count;

                InserirEmBatches(db, opts, novos, onProgress, totalEsperado);
                AtualizarEmBatches(db, opts, atualizacoes, onProgress, totalEsperado, jaProcessados: novos.Count);
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

    private static void InserirEmBatches(
        AppDbContext db, ImportOptions opts, List<EstruturaProduto> entidades,
        Action<int, int>? onProgress, int totalEsperado, int jaProcessados = 0)
    {
        int processados = jaProcessados;
        for (int i = 0; i < entidades.Count; i += opts.BatchSize)
        {
            var batch = entidades.Skip(i).Take(opts.BatchSize).ToList();
            db.EstruturasProdutos.AddRange(batch);
            db.SaveChanges();
            db.ChangeTracker.Clear();

            processados += batch.Count;
            onProgress?.Invoke(processados, totalEsperado);
        }
    }

    private static void AtualizarEmBatches(
        AppDbContext db, ImportOptions opts,
        List<(EstruturaProduto Atual, EstruturaProduto Novo)> pares,
        Action<int, int>? onProgress, int totalEsperado, int jaProcessados)
    {
        int processados = jaProcessados;
        for (int i = 0; i < pares.Count; i += opts.BatchSize)
        {
            var batch = pares.Skip(i).Take(opts.BatchSize).ToList();
            foreach (var (atual, novo) in batch)
            {
                db.Attach(atual);
                atual.Quantidade = novo.Quantidade;
                atual.Posicao = novo.Posicao;
                atual.ModificadoEm = DateTime.UtcNow;
                atual.ModificadoPor = "ProtheusImporter";
            }
            db.SaveChanges();
            db.ChangeTracker.Clear();

            processados += batch.Count;
            onProgress?.Invoke(processados, totalEsperado);
        }
    }

    /// <summary>
    /// "001" -> 1, "027" -> 27, "" -> 0. Falha vira 0.
    /// Protheus geralmente usa inteiros com zero-padding ("001").
    /// </summary>
    private static int ParseRevFinal(string raw)
    {
        var v = raw.Trim();
        if (string.IsNullOrEmpty(v)) return 0;
        return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0;
    }

    /// <summary>
    /// Converte "1,5" ou "1.5" ou "1" em decimal. Erro vira 0.
    /// </summary>
    private static decimal ParseQuantidade(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0m;

        var v = raw.Trim().Replace(',', '.');
        return decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) ? dec : 0m;
    }

    /// <summary>
    /// "0515" -> 515. Falha vira 0.
    /// </summary>
    private static int ParseOrdemItem(string raw)
    {
        var v = raw.Trim();
        if (string.IsNullOrEmpty(v)) return 0;
        return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0;
    }
}
