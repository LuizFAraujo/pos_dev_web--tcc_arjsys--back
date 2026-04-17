using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

/// <summary>
/// Entrada — criar Pedido de Venda.
/// Status opcional: Aguardando (venda futura) ou EmAndamento (default, venda realizada).
/// </summary>
public class PedidoVendaCreateDTO
{
    /// <summary>FK para o Cliente</summary>
    public int ClienteId { get; set; }

    /// <summary>Observações do pedido (opcional)</summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Status inicial. Aguardando = venda futura, EmAndamento = venda realizada (default).
    /// Se omitido, default EmAndamento.
    /// </summary>
    public StatusPedidoVenda? Status { get; set; }
}

public class PedidoVendaResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public StatusPedidoVenda Status { get; set; }
    public DateTime Data { get; set; }
    public string? Observacoes { get; set; }
    public decimal Total { get; set; }
    public List<PedidoVendaItemResponseDTO> Itens { get; set; } = [];
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

public class StatusPedidoVendaDTO
{
    public StatusPedidoVenda NovoStatus { get; set; }
}