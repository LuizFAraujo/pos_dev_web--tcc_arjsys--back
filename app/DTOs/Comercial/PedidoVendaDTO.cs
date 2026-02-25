using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

public class PedidoVendaCreateDTO
{
    public int ClienteId { get; set; }
    public string? Observacoes { get; set; }
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