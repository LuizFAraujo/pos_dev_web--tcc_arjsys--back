using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

/// <summary>
/// Entrada — criar Número de Série.
/// Tipo e status opcionais com defaults inteligentes.
/// </summary>
public class NumeroSerieCreateDTO
{
    /// <summary>FK para o Pedido de Venda</summary>
    public int PedidoVendaId { get; set; }

    /// <summary>Normal (default) ou VendaFutura</summary>
    public TipoNumeroSerie Tipo { get; set; } = TipoNumeroSerie.Normal;

    /// <summary>
    /// Status inicial. Se não informado:
    /// VendaFutura → Aguardando,
    /// Normal → EmAndamento
    /// </summary>
    public StatusNumeroSerie? Status { get; set; }

    /// <summary>Código do projeto da engenharia (opcional)</summary>
    public string? CodigoProjeto { get; set; }
}

/// <summary>Saída — retorno dos endpoints</summary>
public class NumeroSerieResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int PedidoVendaId { get; set; }
    public string PedidoVendaCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public TipoNumeroSerie Tipo { get; set; }
    public StatusNumeroSerie Status { get; set; }
    public string? CodigoProjeto { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

/// <summary>Entrada — alterar status do NS</summary>
public class StatusNumeroSerieDTO
{
    public StatusNumeroSerie NovoStatus { get; set; }
}