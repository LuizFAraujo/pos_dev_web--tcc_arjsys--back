using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Models.Producao;

/// <summary>
/// Item da Ordem de Produção — snapshot de um produto da BOM.
/// QuantidadePlanejada é fixa (snapshot no momento da criação).
/// QuantidadeProduzida cresce conforme apontamentos da produção.
/// Se a BOM mudar após a OP ser criada, a diferença é detectada pelo endpoint
/// de divergência — a OP em si não se auto-ajusta (snapshot).
/// </summary>
public class OrdemProducaoItem : BaseEntity
{
    /// <summary>FK para a Ordem de Produção</summary>
    public int OrdemProducaoId { get; set; }
    public OrdemProducao OrdemProducao { get; set; } = null!;

    /// <summary>FK para o Produto (da BOM do Master)</summary>
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    /// <summary>Quantidade planejada (snapshot da BOM × quantidade da Master)</summary>
    public decimal QuantidadePlanejada { get; set; }

    /// <summary>Quantidade efetivamente produzida (cresce via apontamentos)</summary>
    public decimal QuantidadeProduzida { get; set; }

    /// <summary>Observação do item (opcional)</summary>
    public string? Observacao { get; set; }
}
