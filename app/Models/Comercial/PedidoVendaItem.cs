using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Models.Comercial;

public class PedidoVendaItem : BaseEntity
{
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}