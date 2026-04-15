namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Status do Pedido de Venda.
/// Aguardando = pré-pedido (venda futura, depende de aprovação externa).
/// EmAndamento = venda realizada, engenharia e produção trabalhando.
/// AguardandoEntrega = tudo fabricado, aguardando logística/transporte.
/// </summary>
public enum StatusPedidoVenda
{
    Aguardando,
    EmAndamento,
    Pausado,
    Concluido,
    AguardandoEntrega,
    Entregue,
    Cancelado
}