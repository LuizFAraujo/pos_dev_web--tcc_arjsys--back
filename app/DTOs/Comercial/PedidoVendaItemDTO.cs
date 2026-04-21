namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class PedidoVendaItemDTO { }

/// <summary>
/// Entrada — criar/editar item do PV.
/// Item é descrição livre, sem vínculo com Produto cadastrado.
/// Justificativa é obrigatória quando o PV está em status avançado
/// (Andamento, Concluido, AEntregar, Pausado).
/// </summary>
public class PedidoVendaItemCreateDTO
{
    /// <summary>Quantidade (deve ser maior que zero)</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Descrição livre do item (obrigatória)</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Observação adicional do item (ex: marca, cor)</summary>
    public string? Observacao { get; set; }

    /// <summary>
    /// Justificativa da alteração. Ignorada em status iniciais.
    /// Obrigatória em Andamento, Concluido, AEntregar e Pausado.
    /// </summary>
    public string? Justificativa { get; set; }
}

/// <summary>
/// Entrada — usado no PUT consolidado do PV.
/// Id preenchido = atualiza item existente; Id null/0 = cria novo.
/// Itens no banco que não estão nesta lista são deletados.
/// </summary>
public class PedidoVendaItemUpsertDTO
{
    /// <summary>Se preenchido, atualiza item existente. Se null/0, cria novo.</summary>
    public int? Id { get; set; }

    /// <summary>Quantidade (deve ser maior que zero)</summary>
    public decimal Quantidade { get; set; }

    /// <summary>Descrição livre do item (obrigatória)</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Observação adicional do item</summary>
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
