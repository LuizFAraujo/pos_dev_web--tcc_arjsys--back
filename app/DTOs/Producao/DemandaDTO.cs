using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.DTOs.Producao;

// Classe âncora pra evitar rename automático do VS Code.
public partial class DemandaDTO { }

/// <summary>
/// Linha flat da Demanda de Produção. Cada linha = 1 item de OP ativa
/// (par OP × produto). OPs Concluida e Cancelada não entram.
/// Linhas com QuantidadeFaltante &lt;= 0 são excluídas.
/// </summary>
public class DemandaItemDTO
{
    public int OrdemProducaoId { get; set; }
    public string OrdemProducaoCodigo { get; set; } = string.Empty;
    public StatusOrdemProducao StatusOp { get; set; }
    public int OrdemProducaoItemId { get; set; }

    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;
    public string ProdutoUnidade { get; set; } = string.Empty;
    public TipoProduto TipoProduto { get; set; }

    public decimal QuantidadePlanejada { get; set; }
    public decimal QuantidadeProduzida { get; set; }
    public decimal QuantidadeFaltante { get; set; }
    public decimal PercentualConcluido { get; set; }
}
