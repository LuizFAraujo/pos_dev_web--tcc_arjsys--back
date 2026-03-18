namespace Api_ArjSys_Tcc.Models.Engenharia;

/// <summary>
/// Estrutura de produto (BOM — Bill of Materials).
/// Relacionamento pai-filho entre produtos com quantidade e posição.
/// </summary>
public class EstruturaProduto : BaseEntity
{
    /// <summary>FK do produto pai (montagem/conjunto)</summary>
    public int ProdutoPaiId { get; set; }
    public Produto ProdutoPai { get; set; } = null!;

    /// <summary>FK do produto filho (componente/peça)</summary>
    public int ProdutoFilhoId { get; set; }
    public Produto ProdutoFilho { get; set; } = null!;

    /// <summary>Quantidade do filho necessária para 1 unidade do pai</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Posição na lista de materiais (automática)</summary>
    public int Posicao { get; set; }

    /// <summary>Observação opcional sobre o item na estrutura</summary>
    public string? Observacao { get; set; }
}