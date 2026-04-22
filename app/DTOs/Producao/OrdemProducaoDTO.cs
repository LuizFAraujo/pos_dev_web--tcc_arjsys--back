using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.DTOs.Producao;

// Classe âncora — evita rename automático do VS Code
public partial class OrdemProducaoDTO { }

/// <summary>
/// Entrada — criar OP Master. PV é OPCIONAL (null = OP de estoque/independente).
/// Se PV informado, deve estar em Liberado, Andamento ou Pausado.
/// </summary>
public class OrdemProducaoMasterCreateDTO
{
    public int? PedidoVendaId { get; set; }
    public int ProdutoId { get; set; }
    public string? Observacoes { get; set; }
}

/// <summary>
/// Entrada — criar OP Filha. Herda PV do Master, define Produto da BOM.
/// </summary>
public class OrdemProducaoFilhaCreateDTO
{
    public int OrdemPaiId { get; set; }
    public int ProdutoId { get; set; }
    public string? Observacoes { get; set; }
}

public class OrdemProducaoUpdateDTO
{
    public string? Observacoes { get; set; }
}

public class OrdemProducaoStatusDTO
{
    public StatusOrdemProducao NovoStatus { get; set; }
    public string? Justificativa { get; set; }
}

public class OrdemProducaoApontamentoDTO
{
    public decimal Quantidade { get; set; }
    public string? Observacao { get; set; }
}

/// <summary>
/// Response da OP. PV e Cliente opcionais (OP de estoque pode não ter PV).
/// </summary>
public class OrdemProducaoResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int? PedidoVendaId { get; set; }
    public string? PedidoVendaCodigo { get; set; }

    /// <summary>Código humano do cliente do PV (ex: "CLI-0042"). Null se OP é de estoque.</summary>
    public string? ClienteCodigo { get; set; }

    public string? ClienteNome { get; set; }

    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;

    public int? OrdemPaiId { get; set; }
    public string? OrdemPaiCodigo { get; set; }
    public bool EhMaster => OrdemPaiId == null;
    public bool EhEstoque => PedidoVendaId == null;

    public StatusOrdemProducao Status { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacoes { get; set; }

    public List<OrdemProducaoItemResponseDTO> Itens { get; set; } = [];
    public List<OrdemProducaoFilhaResumoDTO> Filhas { get; set; } = [];

    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}

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

public class OrdemProducaoStatusProducaoDTO
{
    public int OrdemProducaoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public StatusOrdemProducao Status { get; set; }
    public List<OrdemProducaoItemResponseDTO> Itens { get; set; } = [];
    public decimal PercentualTotal { get; set; }
    public bool TudoProduzido { get; set; }
}

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
