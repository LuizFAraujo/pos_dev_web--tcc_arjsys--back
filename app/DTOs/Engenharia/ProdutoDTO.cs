using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.DTOs.Engenharia;

// Entrada — usado no POST e PUT
public class ProdutoCreateDTO
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCompleta { get; set; }
    public UnidadeMedida Unidade { get; set; } = UnidadeMedida.UN;
    public TipoProduto Tipo { get; set; } = TipoProduto.Fabricado;
    public decimal? Peso { get; set; }
    public bool Ativo { get; set; } = true;
}

// Saída — retornado pela API
public class ProdutoResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCompleta { get; set; }
    public UnidadeMedida Unidade { get; set; }
    public TipoProduto Tipo { get; set; }
    public decimal? Peso { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}