namespace Api_ArjSys_Tcc.DTOs.Engenharia;

/// <summary>
/// Item consolidado da BOM totalmente explodida.
/// Cada produto aparece uma única vez, com a quantidade total somada
/// de todas as ocorrências em todos os níveis da estrutura.
/// </summary>
public class BomExplosaoItemDTO
{
    public int ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal QuantidadeTotal { get; set; }
}

/// <summary>
/// Resposta da explosão de BOM de um produto.
/// </summary>
public class BomExplosaoResponseDTO
{
    public int ProdutoPaiId { get; set; }
    public string ProdutoPaiCodigo { get; set; } = string.Empty;
    public string ProdutoPaiDescricao { get; set; } = string.Empty;

    /// <summary>Itens folha consolidados (cada um aparece 1 vez com qtd total)</summary>
    public List<BomExplosaoItemDTO> Itens { get; set; } = [];

    /// <summary>Total de linhas retornadas</summary>
    public int TotalItens { get; set; }
}
