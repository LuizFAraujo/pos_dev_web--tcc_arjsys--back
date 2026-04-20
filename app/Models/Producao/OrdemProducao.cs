using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.Models.Producao;

/// <summary>
/// Ordem de Produção.
/// Código no formato OP.AAAA.MM.NNNN para Master.
/// Filhas reaproveitam o código do Master + sequencial: OP.AAAA.MM.NNNN/NNNN.
///
/// Hierarquia:
/// - Master (OrdemPaiId = null): agrupa OPs filhas. Liga ao PV + Produto raiz.
/// - Filha (OrdemPaiId != null): produz um produto específico da estrutura do Master.
///
/// Master e Filhas têm status independentes — o andamento de uma Master é derivado
/// do status de suas filhas (consultar a lista).
/// </summary>
public class OrdemProducao : BaseEntity
{
    /// <summary>Código único da OP (formato OP.AAAA.MM.NNNN ou OP.AAAA.MM.NNNN/NNNN)</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>FK para o Pedido de Venda — obrigatório na Master, herdado pelas filhas</summary>
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    /// <summary>FK para o Produto — Master: raiz da BOM; Filha: produto específico da estrutura</summary>
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    /// <summary>FK para a OP Master (null = é Master; preenchido = é Filha)</summary>
    public int? OrdemPaiId { get; set; }
    public OrdemProducao? OrdemPai { get; set; }

    /// <summary>Status da OP (independente entre Master e Filha)</summary>
    public StatusOrdemProducao Status { get; set; } = StatusOrdemProducao.Pendente;

    /// <summary>Data de início efetivo da produção (preenchida ao passar para Andamento)</summary>
    public DateTime? DataInicio { get; set; }

    /// <summary>Data de fim efetivo (preenchida ao passar para Concluida ou Cancelada)</summary>
    public DateTime? DataFim { get; set; }

    /// <summary>Observações gerais</summary>
    public string? Observacoes { get; set; }
}
