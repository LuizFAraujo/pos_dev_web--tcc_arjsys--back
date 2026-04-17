namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Eventos registrados no histórico do Pedido de Venda.
/// </summary>
public enum EventoPedidoVenda
{
    Criado,
    Aprovado,
    Pausado,
    Retomado,
    Cancelado,
    Concluido,
    AguardandoEntrega,
    Entregue
}