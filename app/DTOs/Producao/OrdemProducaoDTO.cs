using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.DTOs.Producao;

// Classe âncora — evita rename automático do VS Code
public partial class OrdemProducaoDTO { }

/// <summary>
/// Entrada — criar OP Master. Liga ao PV + Produto raiz da BOM.
/// OrdemPaiId = null (Master). Para criar filha, usar CriarFilhaDTO.
/// </summary>
public class OrdemProducaoMasterCreateDTO
{
    public int PedidoVendaId { get; set; }
    public int ProdutoId { get; set; }
    public string? Observacoes { get; set; }
}

/// <summary>
/// Entrada — criar OP Filha. Herda PV do Master, define Produto da BOM.
/// QuantidadePlanejada do item é definida automaticamente (snapshot da BOM).
/// </summary>
public class OrdemProducaoFilhaCreateDTO
{
    public int OrdemPaiId { get; set; }
    public int ProdutoId { get; set; }
    public string? Observacoes { get; set; }
}

/// <summary>
/// Entrada — editar dados cadastrais da OP.
/// </summary>
public class OrdemProducaoUpdateDTO
{
    public string? Observacoes { get; set; }
}

/// <summary>
/// Entrada — alterar status da OP.
/// Justificativa obrigatória em Pausada e Cancelada.
/// </summary>
public class OrdemProducaoStatusDTO
{
    public StatusOrdemProducao NovoStatus { get; set; }
    public string? Justificativa { get; set; }
}

/// <summary>
/// Entrada — apontar produção em um item.
/// </summary>
public class OrdemProducaoApontamentoDTO
{
    public decimal Quantidade { get; set; }
    public string? Observacao { get; set; }
}

/// <summary>
/// Saída — retorno dos endpoints de OP.
/// Inclui dados readonly do PV, Produto e lista de itens e filhas.
/// </summary>
public class OrdemProducaoResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int PedidoVendaId { get; set; }
    public string PedidoVendaCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;

    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;

    public int? OrdemPaiId { get; set; }
    public string? OrdemPaiCodigo { get; set; }
    public bool EhMaster => OrdemPaiId == null;

    public StatusOrdemProducao Status { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacoes { get; set; }

    public List<OrdemProducaoItemResponseDTO> Itens { get; set; } = [];
    public List<OrdemProducaoFilhaResumoDTO> Filhas { get; set; } = [];

    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

/// <summary>
/// Resumo de uma OP Filha (usado na listagem de filhas da Master).
/// </summary>
public class OrdemProducaoFilhaResumoDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;
    public StatusOrdemProducao Status { get; set; }
    public decimal PercentualConcluido { get; set; }
}

/// <summary>
/// Item da OP na response.
/// </summary>
public class OrdemProducaoItemResponseDTO
{
    public int Id { get; set; }
    public int OrdemProducaoId { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;
    public string ProdutoUnidade { get; set; } = string.Empty;
    public decimal QuantidadePlanejada { get; set; }
    public decimal QuantidadeProduzida { get; set; }
    public decimal QuantidadeFaltante => QuantidadePlanejada - QuantidadeProduzida;
    public decimal PercentualConcluido =>
        QuantidadePlanejada > 0 ? Math.Round(QuantidadeProduzida / QuantidadePlanejada * 100m, 2) : 0m;
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

/// <summary>
/// Status de produção consolidado de uma OP (endpoint dedicado).
/// </summary>
public class OrdemProducaoStatusProducaoDTO
{
    public int OrdemProducaoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public StatusOrdemProducao Status { get; set; }
    public List<OrdemProducaoItemResponseDTO> Itens { get; set; } = [];
    public decimal PercentualTotal { get; set; }
    public bool TudoProduzido { get; set; }
}

/// <summary>
/// Divergência entre OP × BOM atual (endpoint dedicado).
/// </summary>
public class OrdemProducaoDivergenciaItemDTO
{
    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;
    public decimal QuantidadeNaOp { get; set; }
    public decimal QuantidadeNaBomAtual { get; set; }
    public decimal Diferenca { get; set; }
    public string Observacao { get; set; } = string.Empty;
}

public class OrdemProducaoDivergenciaDTO
{
    public int OrdemProducaoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public bool TemDivergencia { get; set; }
    public List<OrdemProducaoDivergenciaItemDTO> Divergencias { get; set; } = [];
}

/// <summary>
/// Entrada do histórico.
/// </summary>
public partial class OrdemProducaoHistoricoDTO { }

public class OrdemProducaoHistoricoResponseDTO
{
    public int Id { get; set; }
    public int OrdemProducaoId { get; set; }
    public EventoOrdemProducao Evento { get; set; }
    public StatusOrdemProducao? StatusAnterior { get; set; }
    public StatusOrdemProducao? StatusNovo { get; set; }
    public string? Justificativa { get; set; }
    public string? Detalhe { get; set; }
    public DateTime DataHora { get; set; }
}
