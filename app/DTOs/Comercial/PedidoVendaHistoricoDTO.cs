using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class PedidoVendaHistoricoDTO { }

/// <summary>
/// Saída — registro de evento no histórico do PV.
/// </summary>
public class PedidoVendaHistoricoResponseDTO
{
    public int Id { get; set; }
    public int PedidoVendaId { get; set; }
    public EventoPedidoVenda Evento { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
}