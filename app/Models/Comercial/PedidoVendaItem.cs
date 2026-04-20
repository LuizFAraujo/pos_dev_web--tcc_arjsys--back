namespace Api_ArjSys_Tcc.Models.Comercial;

/// <summary>
/// Item do Pedido de Venda — descrição livre.
/// Sem vínculo com Produto cadastrado. A vinculação técnica acontece na engenharia (NS/projeto).
/// </summary>
public class PedidoVendaItem : BaseEntity
{
    /// <summary>FK para o Pedido de Venda</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>Quantidade do item</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Descrição livre do item (ex: "Motor Trifásico 150KW")</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Observação do item (ex: marca, cor, especificação adicional)</summary>
    public string? Observacao { get; set; }
}
