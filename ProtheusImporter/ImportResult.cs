namespace ProtheusImporter.Core;

/// <summary>
/// Item repetido dentro do CSV, agrupado pela chave (codigo unico no SB1,
/// tripla pai/filho/sequencia no SG1). Guarda o numero de todas as linhas
/// onde a mesma chave apareceu.
/// </summary>
public sealed class ItemRepetido
{
    /// <summary>Texto principal (ex.: "30.GRD.FR01.005.EC3T").</summary>
    public string Chave { get; set; } = string.Empty;

    /// <summary>Texto auxiliar (ex.: no SG1 -> "COMP.: X - SEQ.: Y"). Opcional.</summary>
    public string? Detalhe { get; set; }

    /// <summary>Numero das linhas do CSV onde essa chave apareceu (na ordem).</summary>
    public List<int> Linhas { get; } = new();
}

/// <summary>
/// Item rejeitado por nao ter referencia em SB1 (produtos). Usado apenas pelo SG1
/// pra detalhar cada linha com status de pai e filho.
/// </summary>
public sealed class RejeicaoSG1
{
    public int Linha { get; set; }
    public string CodigoPai { get; set; } = string.Empty;
    public string CodigoComponente { get; set; } = string.Empty;
    public bool PaiEncontrado { get; set; }
    public bool ComponenteEncontrado { get; set; }
}

/// <summary>
/// Resultado consolidado de uma importacao.
/// </summary>
public sealed class ImportResult
{
    /// <summary>Nome da importacao (ex.: "SB1 - Produtos", "SG1 - Estruturas").</summary>
    public string TipoImportacao { get; set; } = string.Empty;

    /// <summary>Caminho completo do CSV importado.</summary>
    public string ArquivoCsv { get; set; } = string.Empty;

    /// <summary>Modo usado (UPSERT ou Reset).</summary>
    public string Modo { get; set; } = string.Empty;

    /// <summary>Quando a importacao foi iniciada (pro cabecalho do relatorio).</summary>
    public DateTime QuandoIniciou { get; set; } = DateTime.Now;

    /// <summary>Total de linhas de dados lidas do CSV (excluindo header).</summary>
    public int LinhasLidas { get; set; }

    /// <summary>Registros novos que serao/foram inseridos.</summary>
    public int Novos { get; set; }

    /// <summary>Registros existentes que serao/foram atualizados.</summary>
    public int Atualizados { get; set; }

    /// <summary>Registros que ja existiam identicos (sem mudanca detectada).</summary>
    public int Inalterados { get; set; }

    /// <summary>Total de linhas duplicadas dentro do CSV (soma das ocorrencias extras).</summary>
    public int RepetidosTotal { get; set; }

    /// <summary>Linhas rejeitadas (FK inexistente, dado invalido, etc.).</summary>
    public int Rejeitados { get; set; }

    /// <summary>
    /// SG1 especifico: total de codigos-pai distintos no CSV que tinham pelo
    /// menos uma linha valida apos filtro de Rev. Final.
    /// </summary>
    public int PaisComRevisao { get; set; }

    /// <summary>
    /// SG1 especifico: quantos desses pais tiveram mais de uma Rev. Final no CSV
    /// (ou seja, onde o filtro de "ultima revisao" realmente descartou linhas).
    /// </summary>
    public int PaisFiltrados { get; set; }

    /// <summary>
    /// SG1 especifico: total de linhas do CSV descartadas por pertencer a uma
    /// Rev. Final mais antiga que a maxima do pai. Essas NAO aparecem em rejeitados.
    /// </summary>
    public int LinhasDescartadasRevAntiga { get; set; }

    /// <summary>
    /// Repetidos agrupados por chave (CSV inteiro). Cada entrada contem as linhas
    /// onde a mesma chave apareceu. Usado no relatorio na secao "REPETIDOS NO CSV".
    /// </summary>
    public List<ItemRepetido> Repetidos { get; } = new();

    /// <summary>
    /// Rejeicoes detalhadas do SG1 (pai/filho com status encontrado/nao encontrado).
    /// Vazio quando nao aplicavel (ex.: SB1 usa Mensagens genericas).
    /// </summary>
    public List<RejeicaoSG1> RejeicoesSG1 { get; } = new();

    /// <summary>
    /// Rejeicoes/mensagens genericas (usado pelo SB1 e pra erros gerais do SG1
    /// que nao se encaixam em RejeicoesSG1, como "dados vazios").
    /// </summary>
    public List<string> Mensagens { get; } = new();

    /// <summary>Duracao total (preenchida ao final pelo importer).</summary>
    public TimeSpan Duracao { get; set; }
}
