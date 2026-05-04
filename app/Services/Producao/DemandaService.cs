using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.DTOs.Producao;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.Services.Producao;

/// <summary>
/// Serviço de Demanda de Produção - visão flat dos itens de OPs ativas.
/// Cada linha = 1 par (OP × produto). Usado pra responder "o que precisa
/// ser fabricado/comprado neste momento".
///
/// OPs ativas = status Pendente, Andamento, Pausada.
/// OPs Concluida e Cancelada não entram.
/// </summary>
public class DemandaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista a demanda flat, opcionalmente filtrada por tipos.
    /// Se tipos for null/vazio, retorna todos os tipos.
    /// Linhas com QuantidadeFaltante &lt;= 0 são excluídas (já produzido).
    /// </summary>
    public async Task<List<DemandaItemDTO>> Listar(IEnumerable<TipoProduto>? tipos = null)
    {
        var tiposSet = tipos?.ToHashSet();

        var query = _context.OrdensProducaoItens
            .Include(i => i.Produto)
            .Include(i => i.OrdemProducao)
            .Where(i =>
                i.OrdemProducao.Status != StatusOrdemProducao.Concluida &&
                i.OrdemProducao.Status != StatusOrdemProducao.Cancelada);

        if (tiposSet != null && tiposSet.Count > 0)
            query = query.Where(i => tiposSet.Contains(i.Produto.Tipo));

        var itens = await query.ToListAsync();

        return itens
            .Select(i =>
            {
                var faltante = i.QuantidadePlanejada - i.QuantidadeProduzida;
                var pct = i.QuantidadePlanejada > 0
                    ? Math.Round(i.QuantidadeProduzida / i.QuantidadePlanejada * 100m, 2)
                    : 0m;

                return new DemandaItemDTO
                {
                    OrdemProducaoId = i.OrdemProducaoId,
                    OrdemProducaoCodigo = i.OrdemProducao.Codigo,
                    StatusOp = i.OrdemProducao.Status,
                    OrdemProducaoItemId = i.Id,

                    ProdutoId = i.ProdutoId,
                    ProdutoCodigo = i.Produto.Codigo,
                    ProdutoDescricao = i.Produto.Descricao,
                    ProdutoUnidade = i.Produto.Unidade.ToString(),
                    TipoProduto = i.Produto.Tipo,

                    QuantidadePlanejada = i.QuantidadePlanejada,
                    QuantidadeProduzida = i.QuantidadeProduzida,
                    QuantidadeFaltante = faltante,
                    PercentualConcluido = pct
                };
            })
            .Where(d => d.QuantidadeFaltante > 0)
            .OrderBy(d => d.OrdemProducaoCodigo)
            .ThenBy(d => d.ProdutoCodigo)
            .ToList();
    }
}
