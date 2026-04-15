using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

public class NumeroSerie : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;
    public StatusNumeroSerie Status { get; set; } = StatusNumeroSerie.Aguardando;
}