namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Status possíveis de um Pedido de Venda.
///
/// Status de fluxo (automáticos ou manuais conforme o ponto do processo):
/// - AguardandoNS        : PV tipo PreVenda recém criado, aguardando Engenharia gerar NS
/// - RecebidoNS          : NS gerado pela Engenharia, Comercial avalia com cliente
/// - AguardandoRetorno   : Comercial entregou proposta/NS ao cliente e aguarda retorno
/// - Liberado            : Comercial liberou para produção (PreVenda entra aqui após fluxo; Normal inicia aqui)
/// - Andamento           : Produção iniciou a OP
/// - Concluido           : Produção finalizou a OP
/// - AEntregar           : Comercial liberou para expedição
/// - Entregue            : Entrega confirmada (status terminal do fluxo normal)
///
/// Status especiais (sempre manuais, exigem justificativa + data):
/// - Pausado             : Fluxo interrompido temporariamente
/// - Cancelado           : PV cancelado (pode vir de qualquer status, exceto Entregue)
/// - Reaberto            : PV saiu do Cancelado; permanece Reaberto até Comercial movê-lo manualmente para Liberado
/// - Devolvido           : Só pode vir de Entregue (cliente devolveu)
/// </summary>
public enum StatusPedidoVenda
{
    // Fluxo PreVenda (3 status iniciais)
    AguardandoNS,
    RecebidoNS,
    AguardandoRetorno,

    // Fluxo comum (Normal inicia em Liberado; PreVenda chega aqui após AguardandoRetorno)
    Liberado,
    Andamento,
    Concluido,
    AEntregar,
    Entregue,

    // Especiais
    Pausado,
    Cancelado,
    Reaberto,
    Devolvido
}
