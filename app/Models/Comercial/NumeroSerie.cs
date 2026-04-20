namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Número de Série — identificador único de projeto/fabricação.
/// Relação 1:1 com Pedido de Venda.
/// Código no formato II.MM.AA.NNNNN (idade empresa.mês.ano.sequencial).
/// Tipo e Status são herdados do PV vinculado (exibição readonly no response).
/// </summary>
public class NumeroSerie : BaseEntity
{
    /// <summary>Código único do NS (formato II.MM.AA.NNNNN)</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>FK para o Pedido de Venda vinculado (1:1)</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>Código do projeto criado pela engenharia (opcional)</summary>
    public string? CodigoProjeto { get; set; }
}
