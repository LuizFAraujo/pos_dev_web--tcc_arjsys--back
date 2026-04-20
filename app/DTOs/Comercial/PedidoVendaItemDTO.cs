namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class PedidoVendaItemDTO { }

/// <summary>
/// Entrada — criar/editar item do PV.
/// Item é descrição livre, sem vínculo com Produto cadastrado.
/// </summary>
public class PedidoVendaItemCreateDTO
{
    /// <summary>Quantidade (deve ser maior que zero)</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Descrição livre do item (obrigatória)</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Observação adicional do item (ex: marca, cor)</summary>
    public string? Observacao { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints de itens.
/// </summary>
public class PedidoVendaItemResponseDTO
{
    public int Id { get; set; }
    public int PedidoVendaId { get; set; }
    public decimal Quantidade { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
