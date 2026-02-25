using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

public class NumeroSerieCreateDTO
{
    public int PedidoVendaId { get; set; }
}

public class NumeroSerieResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int PedidoVendaId { get; set; }
    public string PedidoVendaCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public StatusNumeroSerie Status { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

public class StatusNumeroSerieDTO
{
    public StatusNumeroSerie NovoStatus { get; set; }
}