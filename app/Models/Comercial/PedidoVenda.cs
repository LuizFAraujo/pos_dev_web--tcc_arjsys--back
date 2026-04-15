using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

public class PedidoVenda : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public StatusPedidoVenda Status { get; set; } = StatusPedidoVenda.EmAndamento;
    public DateTime Data { get; set; }
    public string? Observacoes { get; set; }
}