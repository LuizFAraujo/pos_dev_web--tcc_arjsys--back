using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Pedido de Venda. Código no formato PV.AAAA.MM.NNNN.
/// Tipo Normal = venda realizada, VendaFutura = pré-pedido.
/// Data = data de negócio do pedido (editável). CriadoEm = auditoria do registro.
/// </summary>
public class PedidoVenda : BaseEntity
{
    /// <summary>Código único do PV (formato PV.AAAA.MM.NNNN)</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>FK para o Cliente</summary>
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Normal ou VendaFutura</summary>
    public TipoPedidoVenda Tipo { get; set; } = TipoPedidoVenda.Normal;

    /// <summary>Status atual do PV</summary>
    public StatusPedidoVenda Status { get; set; } = StatusPedidoVenda.EmAndamento;

    /// <summary>Data da venda (negócio). Default = momento da criação, editável pelo usuário.</summary>
    public DateTime Data { get; set; }

    /// <summary>Data combinada de entrega (opcional)</summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>Observações gerais do pedido</summary>
    public string? Observacoes { get; set; }
}
