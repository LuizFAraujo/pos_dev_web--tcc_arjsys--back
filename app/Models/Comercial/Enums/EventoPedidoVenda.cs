namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Eventos registrados no histórico do Pedido de Venda.
/// Cada transição de status gera um evento correspondente, com StatusAnterior,
/// StatusNovo, Justificativa (quando aplicável) e DataHora.
/// </summary>
public enum EventoPedidoVenda
{
    /// <summary>PV criado (evento inicial, sem StatusAnterior).</summary>
    Criado,

    /// <summary>NS recebido da Engenharia (PreVenda: AguardandoNS → RecebidoNS).</summary>
    NsRecebido,

    /// <summary>Comercial confirmou recebimento do NS e enviou ao cliente (RecebidoNS → AguardandoRetorno).</summary>
    RetornoSolicitado,

    /// <summary>Cliente aprovou, PV liberado para produção (AguardandoRetorno → Liberado).</summary>
    Aprovado,

    /// <summary>Produção iniciou a OP (Liberado → Andamento).</summary>
    ProducaoIniciada,

    /// <summary>Produção finalizou a OP (Andamento → Concluido).</summary>
    ProducaoConcluida,

    /// <summary>Comercial liberou para expedição (Concluido → AEntregar).</summary>
    LiberadoEntrega,

    /// <summary>Entrega confirmada (AEntregar → Entregue).</summary>
    Entregue,

    /// <summary>PV pausado (manual, qualquer status exceto Entregue).</summary>
    Pausado,

    /// <summary>PV retomado (saiu de Pausado).</summary>
    Retomado,

    /// <summary>PV cancelado (manual, qualquer status exceto Entregue).</summary>
    Cancelado,

    /// <summary>PV reaberto (saiu de Cancelado → Reaberto).</summary>
    Reaberto,

    /// <summary>PV devolvido (só pode vir de Entregue).</summary>
    Devolvido,

    /// <summary>Itens do PV alterados em status avançado (com justificativa obrigatória).</summary>
    ItensAlterados
}
