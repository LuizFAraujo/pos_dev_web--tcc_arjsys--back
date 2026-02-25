using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.Models.Engenharia;

public class Produto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? DescricaoCompleta { get; set; }
    public UnidadeMedida Unidade { get; set; }
    public TipoProduto Tipo { get; set; }
    public decimal? Peso { get; set; }
    public bool Ativo { get; set; } = true;
    public bool TemDocumento { get; set; } = false;
}