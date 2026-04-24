using System.Text;

namespace ProtheusImporter.Core;

/// <summary>
/// Modo de importacao escolhido pelo usuario.
/// </summary>
public enum ModoImportacao
{
    /// <summary>Apaga todos os registros da tabela alvo antes de inserir.</summary>
    ResetarEInserir,

    /// <summary>UPSERT: insere novos, atualiza existentes pela chave natural.</summary>
    AtualizarIncremental
}

/// <summary>
/// Opcoes de leitura e importacao usadas pelo CsvImporter.
/// </summary>
public sealed class ImportOptions
{
    /// <summary>Caminho absoluto do arquivo CSV de origem.</summary>
    public required string CsvPath { get; init; }

    /// <summary>Separador de colunas (ex.: ';' para Protheus).</summary>
    public char Separador { get; init; } = ';';

    /// <summary>Encoding do CSV. Protheus padrao = Windows-1252.</summary>
    public Encoding Encoding { get; init; } = Encoding.GetEncoding(1252);

    /// <summary>Colunas obrigatorias no header (nome exato). Ausencia = erro fatal.</summary>
    public required IReadOnlyList<string> ColunasObrigatorias { get; init; }

    /// <summary>Colunas opcionais no header. Se ausentes, o importer usa default/null.</summary>
    public IReadOnlyList<string> ColunasOpcionais { get; init; } = [];

    /// <summary>Maximo de linhas pra procurar o header. Alem disso, aborta.</summary>
    public int MaxLinhasBuscaHeader { get; init; } = 20;

    /// <summary>Qtd. de registros por SaveChanges.</summary>
    public int BatchSize { get; init; } = 500;

    /// <summary>Modo de importacao (resetar ou upsert).</summary>
    public ModoImportacao Modo { get; init; } = ModoImportacao.AtualizarIncremental;

    /// <summary>
    /// Se true e Modo = ResetarEInserir, permite apagar dependencias em cascata
    /// (ex.: Produto -> Estrutura, NS, OP, OPItens, OPHistorico).
    /// Usar apenas em fase de teste/implantacao inicial — dados em producao serao perdidos.
    /// </summary>
    public bool PermitirResetCascata { get; init; } = false;

    /// <summary>
    /// SG1 especifico: quando o CSV tem mesmo pai+filho com Sequencias DIFERENTES
    /// (caso legitimo no Protheus), como o ARJ ainda nao tem Sequencia na Estrutura,
    /// precisamos decidir:
    ///   true  = soma as Quantidades e grava 1 linha consolidada (workaround atual)
    ///   false = mantem so a ultima ocorrencia (comportamento quando ARJ ja suportar
    ///           Sequencia e quisermos resolver corretamente depois)
    /// Padrao: true.
    /// </summary>
    public bool SomarSequenciasRepetidas { get; init; } = true;
}
