using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.DTOs.Comercial;

// Classe âncora — evita rename automático do VS Code
public partial class NumeroSerieDTO { }

/// <summary>
/// Entrada — criar Número de Série.
/// Vinculação 1:1 com PV (rejeita se o PV já tiver NS).
/// Só pode criar NS para PV com status Aguardando ou EmAndamento.
/// </summary>
public class NumeroSerieCreateDTO
{
    /// <summary>FK para o Pedido de Venda (1:1)</summary>
    public int PedidoVendaId { get; set; }

    /// <summary>Código do projeto da engenharia (opcional)</summary>
    public string? CodigoProjeto { get; set; }
}

/// <summary>
/// Entrada — editar Número de Série.
/// Engenharia edita apenas dados do NS. Dados do PV são readonly.
/// </summary>
public class NumeroSerieUpdateDTO
{
    /// <summary>Código do projeto da engenharia (opcional)</summary>
    public string? CodigoProjeto { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints de NS.
/// Inclui dados readonly do PV vinculado (tipo, status, data entrega, cliente).
/// </summary>
public class NumeroSerieResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int PedidoVendaId { get; set; }
    public string PedidoVendaCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public TipoPedidoVenda PvTipo { get; set; }
    public StatusPedidoVenda PvStatus { get; set; }
    public DateTime? PvDataEntrega { get; set; }
    public string? CodigoProjeto { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
