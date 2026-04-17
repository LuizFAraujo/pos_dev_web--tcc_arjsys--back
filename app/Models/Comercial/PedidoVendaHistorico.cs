using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Log de eventos do Pedido de Venda.
/// Registra cada mudança de status com data/hora e observação opcional.
/// </summary>
public class PedidoVendaHistorico : BaseEntity
{
    /// <summary>FK para o Pedido de Venda</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>Tipo do evento (Criado, Aprovado, Pausado, etc.)</summary>
    public EventoPedidoVenda Evento { get; set; }

    /// <summary>Data/hora do evento</summary>
    public DateTime DataHora { get; set; }

    /// <summary>Observação opcional sobre o evento</summary>
    public string? Observacao { get; set; }
}