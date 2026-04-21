using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class PedidoVendaDTO { }

/// <summary>
/// Entrada — criar Pedido de Venda com itens em chamada única (atômica).
/// Tipo obrigatório: Normal (status inicial Liberado) ou PreVenda (status inicial AguardandoNS).
/// Data da venda opcional — default DateTime.UtcNow.
/// Lista de itens: OBRIGATÓRIA ter pelo menos 1.
/// </summary>
public class PedidoVendaCreateDTO
{
    /// <summary>FK para o Cliente</summary>
    public int ClienteId { get; set; }

    /// <summary>Normal ou PreVenda (obrigatório)</summary>
    public TipoPedidoVenda Tipo { get; set; } = TipoPedidoVenda.Normal;

    /// <summary>Data da venda (negócio). Se omitida, usa a data/hora atual.</summary>
    public DateTime? Data { get; set; }

    /// <summary>Data combinada de entrega (opcional)</summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>Observações do pedido (opcional)</summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Itens do pedido. OBRIGATÓRIO ter pelo menos 1 item.
    /// Lista vazia ou null → 400.
    /// </summary>
    public List<PedidoVendaItemCreateDTO> Itens { get; set; } = [];
}

/// <summary>
/// Entrada — atualizar PV + itens em chamada única com diff sincronizado.
/// Itens com Id são atualizados; sem Id são criados; ausentes da lista são deletados.
/// Em status avançado (Andamento/Concluido/AEntregar/Pausado) justificativa é obrigatória.
/// Dispara evento ItensAlterados no histórico e notifica Engenharia/Produção/Almoxarifado.
/// </summary>
public class PedidoVendaUpdateDTO
{
    /// <summary>FK para o Cliente</summary>
    public int ClienteId { get; set; }

    /// <summary>Normal ou PreVenda</summary>
    public TipoPedidoVenda Tipo { get; set; } = TipoPedidoVenda.Normal;

    /// <summary>Data da venda (opcional)</summary>
    public DateTime? Data { get; set; }

    /// <summary>Data combinada de entrega (opcional)</summary>
    public DateTime? DataEntrega { get; set; }

    /// <summary>Observações do pedido (opcional)</summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Lista completa de itens do pedido (estado final desejado).
    /// OBRIGATÓRIO ter pelo menos 1 item.
    /// </summary>
    public List<PedidoVendaItemUpsertDTO> Itens { get; set; } = [];

    /// <summary>
    /// Justificativa da alteração. Obrigatória quando o PV está em
    /// Andamento, Concluido, AEntregar ou Pausado.
    /// Ignorada em status iniciais.
    /// </summary>
    public string? Justificativa { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints do PV.
/// </summary>
public class PedidoVendaResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public TipoPedidoVenda Tipo { get; set; }
    public StatusPedidoVenda Status { get; set; }
    public DateTime Data { get; set; }
    public DateTime? DataEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<PedidoVendaItemResponseDTO> Itens { get; set; } = [];
    public int TotalItens { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

/// <summary>
/// Entrada — alterar status do PV.
/// Justificativa é obrigatória em Pausar, Cancelar, Reabrir, Devolver e retroceder.
/// </summary>
public class StatusPedidoVendaDTO
{
    /// <summary>Status de destino</summary>
    public StatusPedidoVenda NovoStatus { get; set; }

    /// <summary>Justificativa da mudança. Obrigatória em Pausar/Cancelar/Reabrir/Devolver/retroceder.</summary>
    public string? Justificativa { get; set; }
}
