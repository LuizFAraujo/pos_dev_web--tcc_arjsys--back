using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class NumeroSerieDTO { }

/// <summary>
/// Entrada — criar Número de Série.
/// Vinculação 1:1 com PV (rejeita se o PV já tiver NS).
/// Só pode criar NS para PV tipo PreVenda em status AguardandoNS.
/// ProdutoId é opcional (Engenharia pode preencher depois via Update).
/// </summary>
public class NumeroSerieCreateDTO
{
    /// <summary>FK para o Pedido de Venda (1:1)</summary>
    public int PedidoVendaId { get; set; }

    /// <summary>FK para o Produto BOM (projeto) — opcional</summary>
    public int? ProdutoId { get; set; }
}

/// <summary>
/// Entrada — editar Número de Série.
/// Engenharia edita apenas o Produto vinculado. Dados do PV são readonly.
/// </summary>
public class NumeroSerieUpdateDTO
{
    /// <summary>FK para o Produto BOM (projeto)</summary>
    public int? ProdutoId { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints de NS.
/// Inclui dados readonly do PV vinculado (tipo, status, data entrega, cliente)
/// e do Produto BOM (código, descrição).
/// </summary>
public class NumeroSerieResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    // PV (readonly)
    public int PedidoVendaId { get; set; }
    public string PedidoVendaCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public TipoPedidoVenda PvTipo { get; set; }
    public StatusPedidoVenda PvStatus { get; set; }
    public DateTime? PvDataEntrega { get; set; }

    // Produto BOM (readonly)
    public int? ProdutoId { get; set; }
    public string? ProdutoCodigo { get; set; }
    public string? ProdutoDescricao { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
