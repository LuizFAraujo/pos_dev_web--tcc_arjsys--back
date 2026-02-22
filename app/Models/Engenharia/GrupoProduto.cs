using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.Models.Engenharia;

public class GrupoProduto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public NivelGrupo Nivel { get; set; }
    public int QtdCaracteres { get; set; }
    public string? PathDocumentos { get; set; }
    public bool Ativo { get; set; } = true;
}