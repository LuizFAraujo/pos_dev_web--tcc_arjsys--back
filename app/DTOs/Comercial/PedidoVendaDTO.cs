using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class PedidoVendaDTO { }

/// <summary>
/// Entrada — criar Pedido de Venda.
/// Tipo obrigatório: Normal (status inicial EmAndamento) ou VendaFutura (status inicial Aguardando).
/// Data da venda opcional — default DateTime.UtcNow.
/// </summary>
public class PedidoVendaCreateDTO
{
    /// <summary>FK para o Cliente</summary>
    public int ClienteId { get; set; }

    /// <summary>Normal ou VendaFutura (obrigatório)</summary>
    public TipoPedidoVenda Tipo { get; set; } = TipoPedidoVenda.Normal;

    /// <summary>Data da venda (negócio). Se omitida, usa a data/hora atual.</summary>
    public DateTime? Data { get; set; }

    /// <summary>Data combinada de entrega (opcional)</summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>Observações do pedido (opcional)</summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints do PV.
/// </summary>
public class PedidoVendaResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public TipoPedidoVenda Tipo { get; set; }
    public StatusPedidoVenda Status { get; set; }
    public DateTime Data { get; set; }
    public DateTime? DataEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<PedidoVendaItemResponseDTO> Itens { get; set; } = [];
    public int TotalItens { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

/// <summary>
/// Entrada — alterar status do PV.
/// Justificativa é obrigatória em pausar, cancelar e retroceder (inclui reabertura do Cancelado).
/// </summary>
public class StatusPedidoVendaDTO
{
    /// <summary>Status de destino</summary>
    public StatusPedidoVenda NovoStatus { get; set; }

    /// <summary>Justificativa da mudança. Obrigatória em pausar/cancelar/retroceder.</summary>
    public string? Justificativa { get; set; }
}
