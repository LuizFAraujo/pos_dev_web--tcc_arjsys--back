namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Tipo do Pedido de Venda.
/// Normal = venda já realizada (status inicial EmAndamento).
/// VendaFutura = pré-pedido, depende de aprovação externa (status inicial Aguardando).
/// </summary>
public enum TipoPedidoVenda
{
    Normal,
    VendaFutura
}
