namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Tipo do Pedido de Venda.
/// Define o status inicial e o fluxo que o PV segue.
/// </summary>
public enum TipoPedidoVenda
{
    /// <summary>
    /// Venda regular. Inicia em Liberado e segue direto para produção.
    /// </summary>
    Normal,

    /// <summary>
    /// Pré-venda (tipicamente para liberar NS antes da aprovação de financiamento).
    /// Inicia em AguardandoNS, passa por RecebidoNS e AguardandoRetorno antes de ir para Liberado.
    /// </summary>
    PreVenda
}
