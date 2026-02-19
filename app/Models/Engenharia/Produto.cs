using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.Models.Engenharia;

public class Produto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCompleta { get; set; }
    public UnidadeMedida Unidade { get; set; } = UnidadeMedida.UN;
    public TipoProduto Tipo { get; set; } = TipoProduto.Fabricado;
    public decimal? Peso { get; set; }
    public bool Ativo { get; set; } = true;
}