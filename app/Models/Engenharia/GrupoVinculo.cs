namespace Api_ArjSys_Tcc.Models.Engenharia;

public class GrupoVinculo : BaseEntity
{
    public int GrupoPaiId { get; set; }
    public GrupoProduto GrupoPai { get; set; } = null!;

    public int GrupoFilhoId { get; set; }
    public GrupoProduto GrupoFilho { get; set; } = null!;
}