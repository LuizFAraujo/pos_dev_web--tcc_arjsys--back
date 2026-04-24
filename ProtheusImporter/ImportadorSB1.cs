using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using ProtheusImporter.Core;

namespace ProtheusImporter.Importers;

/// <summary>
/// Importador Protheus SB1 (cadastro de produtos) para Engenharia_Produtos do ARJ.
///
/// Colunas obrigatorias no CSV: Codigo, Descricao
/// Colunas opcionais no CSV:    Blq. de Tela (Ativo), Unidade, Tipo
///
/// Mapeamento de valores:
///   Blq. de Tela: vazio/0 -> Ativo = true; 1 -> Ativo = false
///   Unidade:      se nao bater com enum do ARJ -> default UN
///   Tipo:         MP -> MateriaPrima, PA/PI -> Fabricado, MC -> Comprado,
///                 SV -> Servico, RE -> Revenda; default -> Comprado
/// </summary>
public static class ImportadorSB1
{
    public const string COL_CODIGO = "Codigo";
    public const string COL_DESCRICAO = "Descricao";
    public const string COL_ATIVO = "Blq. de Tela";
    public const string COL_UNIDADE = "Unidade";
    public const string COL_TIPO = "Tipo";

    /// <summary>
    /// Tabelas que dependem de Engenharia_Produtos via FK.
    /// Ordem importa: filhas primeiro, pais depois (historicos antes dos dados).
    /// Usado no reset em cascata.
    /// </summary>
    public static readonly string[] TabelasDependentes =
    {
        "Producao_OrdemProducaoHistorico",
        "Producao_OrdensProducaoItens",
        "Producao_OrdensProducao",
        "Comercial_NumerosSerie",
        "Engenharia_EstruturasProdutos"
    };

    /// <summary>
    /// Monta o CsvImporter tipado pra Produto usando o CSV SB1.
    /// Chave natural = Produto.Codigo (case-sensitive, preserva caracteres crus).
    /// </summary>
    public static CsvImporter<Produto, string> Criar(AppDbContext db, ImportOptions opts)
    {
        return new CsvImporter<Produto, string>(
            db: db,
            opts: opts,
            mapper: MapearLinha,
            chave: p => p.Codigo,
            atualizar: Atualizar,
            keyComparer: StringComparer.Ordinal
        );
    }

    /// <summary>
    /// Conta registros em cada tabela que sera afetada pelo reset em cascata.
    /// Usado pra exibir resumo antes da confirmacao.
    /// Ordem das chaves: dependentes (em ordem de exclusao) + Engenharia_Produtos no fim.
    /// </summary>
    public static Dictionary<string, long> ContarDependentes(AppDbContext db)
    {
        var resultado = new Dictionary<string, long>();

        foreach (var tabela in TabelasDependentes)
        {
            resultado[tabela] = ContarLinhas(db, tabela);
        }

        resultado["Engenharia_Produtos"] = ContarLinhas(db, "Engenharia_Produtos");
        return resultado;
    }

    /// <summary>
    /// Apaga Produtos + todas as dependencias em cascata.
    /// Desliga FK temporariamente pra permitir DELETE em qualquer ordem.
    /// Zera sqlite_sequence das tabelas afetadas pra Ids recomecarem em 1.
    /// </summary>
    public static void ExecutarResetCascata(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

        try
        {
            foreach (var tabela in TabelasDependentes)
            {
                db.Database.ExecuteSqlRaw($"DELETE FROM {tabela};");
            }

            db.Database.ExecuteSqlRaw("DELETE FROM Engenharia_Produtos;");

            // Reseta autoincrement das tabelas afetadas.
            db.Database.ExecuteSqlRaw(
                "DELETE FROM sqlite_sequence WHERE name IN " +
                "('Engenharia_Produtos','Engenharia_EstruturasProdutos'," +
                "'Comercial_NumerosSerie','Producao_OrdensProducao'," +
                "'Producao_OrdensProducaoItens','Producao_OrdemProducaoHistorico');");
        }
        finally
        {
            db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        }
    }

    private static long ContarLinhas(AppDbContext db, string tabela)
    {
        using var cmd = db.Database.GetDbConnection().CreateCommand();
        if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            cmd.Connection.Open();

        cmd.CommandText = $"SELECT COUNT(*) FROM {tabela};";
        var result = cmd.ExecuteScalar();
        return result is null ? 0L : Convert.ToInt64(result);
    }

    /// <summary>
    /// Converte uma linha do CSV SB1 em entidade Produto.
    /// Retorna null quando a linha nao tem codigo (dado invalido).
    /// Preserva caracteres exatos do CSV (sem Trim em Codigo/Descricao).
    /// </summary>
    private static Produto? MapearLinha(CsvRow row)
    {
        var codigo = row.Get(COL_CODIGO);
        if (string.IsNullOrEmpty(codigo)) return null;

        var descricao = row.Get(COL_DESCRICAO);

        return new Produto
        {
            Codigo = codigo,
            Descricao = descricao,
            DescricaoCompleta = null,
            Unidade = ResolverUnidade(row.Get(COL_UNIDADE)),
            Tipo = ResolverTipo(row.Get(COL_TIPO)),
            Peso = null,
            Ativo = ResolverAtivo(row.Get(COL_ATIVO)),
            TemPasta = false,
            TemDocumento = false
        };
    }

    /// <summary>
    /// Atualiza a entidade existente no banco com os dados da nova (vinda do CSV).
    /// NUNCA altera Id nem CriadoEm — preserva origem e auditoria.
    /// </summary>
    private static void Atualizar(Produto atual, Produto novo)
    {
        atual.Descricao = novo.Descricao;
        atual.Unidade = novo.Unidade;
        atual.Tipo = novo.Tipo;
        atual.Ativo = novo.Ativo;
        atual.ModificadoEm = DateTime.UtcNow;
        atual.ModificadoPor = "ProtheusImporter";
    }

    /// <summary>
    /// Regra de Blq. de Tela (Protheus) -> Ativo (ARJ):
    ///   vazio = Ativo (true), 0 = Ativo (true), 1 = Bloqueado (false).
    /// Qualquer outro valor: assume ativo (true) por seguranca.
    /// </summary>
    private static bool ResolverAtivo(string blqTela)
    {
        var v = blqTela.Trim();
        if (string.IsNullOrEmpty(v)) return true;
        if (v == "0") return true;
        if (v == "1") return false;
        return true;
    }

    /// <summary>
    /// Tenta mapear a string de Unidade (Protheus) para o enum UnidadeMedida.
    /// Se nao bater com nenhum valor conhecido, retorna UN (default conservador).
    /// </summary>
    private static UnidadeMedida ResolverUnidade(string raw)
    {
        var v = raw.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(v)) return UnidadeMedida.UN;

        return Enum.TryParse<UnidadeMedida>(v, ignoreCase: true, out var un) ? un : UnidadeMedida.UN;
    }

    /// <summary>
    /// Mapeia codigo de Tipo do Protheus (MP/PA/PI/MC/SV/RE) para enum TipoProduto do ARJ.
    /// Default: Comprado (conservador pra cadastros obscuros).
    /// </summary>
    private static TipoProduto ResolverTipo(string raw)
    {
        var v = raw.Trim().ToUpperInvariant();
        return v switch
        {
            "MP" => TipoProduto.MateriaPrima,
            "PA" => TipoProduto.Fabricado,
            "PI" => TipoProduto.Fabricado,
            "MC" => TipoProduto.Comprado,
            "SV" => TipoProduto.Servico,
            "RE" => TipoProduto.Revenda,
            _ => TipoProduto.Comprado
        };
    }
}
