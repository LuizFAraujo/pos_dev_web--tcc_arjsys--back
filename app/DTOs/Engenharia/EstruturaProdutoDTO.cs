using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.DTOs.Engenharia;

// Entrada — usado no POST e PUT
public class EstruturaProdutoCreateDTO
{
    public int ProdutoPaiId { get; set; }
    public int ProdutoFilhoId { get; set; }
    public decimal Quantidade { get; set; }
    public int Posicao { get; set; }
    public string? Observacao { get; set; }
}

// Saída — retornado pela API
public class EstruturaProdutoResponseDTO
{
    public int Id { get; set; }
    public int ProdutoPaiId { get; set; }
    public int ProdutoFilhoId { get; set; }
    public string ProdutoFilhoCodigo { get; set; } = string.Empty;
    public string ProdutoFilhoDescricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public int Posicao { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

// Saída Flat — exibe pai e filho lado a lado
public class EstruturaProdutoFlatDTO
{
    public int Id { get; set; }
    public int ProdutoPaiId { get; set; }
    public string ProdutoPaiCodigo { get; set; } = string.Empty;
    public string ProdutoPaiDescricao { get; set; } = string.Empty;
    public int ProdutoFilhoId { get; set; }
    public string ProdutoFilhoCodigo { get; set; } = string.Empty;
    public string ProdutoFilhoDescricao { get; set; } = string.Empty;
    public UnidadeMedida ProdutoFilhoUnidade { get; set; }
    public decimal Quantidade { get; set; }
    public int Posicao { get; set; }
    public string? Observacao { get; set; }
}