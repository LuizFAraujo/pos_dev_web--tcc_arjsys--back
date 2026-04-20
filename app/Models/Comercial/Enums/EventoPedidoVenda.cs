namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Eventos registrados no histórico do Pedido de Venda.
/// Reaberto = PV saiu do status Cancelado (reabertura com justificativa).
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
    Entregue,
    Reaberto
}
