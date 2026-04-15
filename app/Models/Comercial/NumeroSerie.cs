using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Número de Série — identificador único de projeto/fabricação.
/// Código no formato II.MM.AA.NNNNN (idade empresa.mês.ano.sequencial).
/// </summary>
public class NumeroSerie : BaseEntity
{
    /// <summary>Código único do NS (formato II.MM.AA.NNNNN)</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>FK para o Pedido de Venda vinculado</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>Normal = venda realizada, VendaFutura = pré-pedido</summary>
    public TipoNumeroSerie Tipo { get; set; } = TipoNumeroSerie.Normal;

    /// <summary>Status atual do NS</summary>
    public StatusNumeroSerie Status { get; set; } = StatusNumeroSerie.Aguardando;

    /// <summary>Código do projeto criado pela engenharia (opcional)</summary>
    public string? CodigoProjeto { get; set; }
}