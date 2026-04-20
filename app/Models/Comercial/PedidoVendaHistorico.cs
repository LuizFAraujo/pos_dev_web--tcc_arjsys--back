using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Log de eventos do Pedido de Venda.
/// Registra cada mudança de status com data/hora, status anterior/novo e justificativa.
/// Justificativa é obrigatória em Pausar, Cancelar e Retroceder (inclui reabertura do Cancelado).
/// </summary>
public class PedidoVendaHistorico : BaseEntity
{
    /// <summary>FK para o Pedido de Venda</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>Tipo do evento (Criado, Aprovado, Pausado, Retomado, Cancelado, Concluido, AguardandoEntrega, Entregue, Reaberto)</summary>
    public EventoPedidoVenda Evento { get; set; }

    /// <summary>Status do PV antes da mudança (null no evento Criado)</summary>
    public StatusPedidoVenda? StatusAnterior { get; set; }

    /// <summary>Status do PV após a mudança</summary>
    public StatusPedidoVenda? StatusNovo { get; set; }

    /// <summary>Justificativa da mudança (obrigatória em pausar/cancelar/retroceder)</summary>
    public string? Justificativa { get; set; }

    /// <summary>Data/hora do evento</summary>
    public DateTime DataHora { get; set; }
}
