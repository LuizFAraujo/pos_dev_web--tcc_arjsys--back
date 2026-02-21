namespace Api_ArjSys_Tcc.Models.Engenharia;

public class EstruturaProduto : BaseEntity
{
    public int ProdutoPaiId { get; set; }
    public Produto ProdutoPai { get; set; } = null!;

    public int ProdutoFilhoId { get; set; }
    public Produto ProdutoFilho { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public int Posicao { get; set; }
    public string? Observacao { get; set; }
}