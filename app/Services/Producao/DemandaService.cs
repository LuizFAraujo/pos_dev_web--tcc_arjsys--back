using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Data.Busca;
using Api_ArjSys_Tcc.DTOs.Producao;
using Api_ArjSys_Tcc.DTOs.Shared;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.Models.Producao;
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
    /// Wrapper para projetar campos calculados (Faltante/Pct) no IQueryable
    /// e habilitar sort/filter server-side sobre eles via BuscaExtensions.
    /// </summary>
    private class DemandaQueryProj
    {
        public OrdemProducaoItem Item { get; set; } = null!;
        public decimal QuantidadeFaltante { get; set; }
        public decimal PercentualConcluido { get; set; }
    }

    /// <summary>
    /// Busca paginada de demanda com filtros, ordenação e busca textual server-side.
    /// Filtros base aplicados sempre: OPs ativas (Pendente/Andamento/Pausada) e
    /// faltante &gt; 0. Demais filtros e ordenações vêm via BuscaRequest.
    /// </summary>
    public async Task<PaginadoResponse<DemandaItemDTO>> Buscar(BuscaRequest req)
    {
        var mapaColunas = new Dictionary<string, string>
        {
            ["ordemProducaoCodigo"] = "Item.OrdemProducao.Codigo",
            ["statusOp"] = "Item.OrdemProducao.Status",
            ["produtoCodigo"] = "Item.Produto.Codigo",
            ["produtoDescricao"] = "Item.Produto.Descricao",
            ["produtoUnidade"] = "Item.Produto.Unidade",
            ["tipoProduto"] = "Item.Produto.Tipo",
            ["quantidadePlanejada"] = "Item.QuantidadePlanejada",
            ["quantidadeProduzida"] = "Item.QuantidadeProduzida",
            ["quantidadeFaltante"] = "QuantidadeFaltante",
            ["percentualConcluido"] = "PercentualConcluido"
        };

        var query = _context.OrdensProducaoItens
            .Include(i => i.Produto)
            .Include(i => i.OrdemProducao)
            .Where(i =>
                i.OrdemProducao.Status != StatusOrdemProducao.Concluida &&
                i.OrdemProducao.Status != StatusOrdemProducao.Cancelada)
            .Where(i => i.QuantidadePlanejada - i.QuantidadeProduzida > 0)
            .Select(i => new DemandaQueryProj
            {
                Item = i,
                QuantidadeFaltante = i.QuantidadePlanejada - i.QuantidadeProduzida,
                PercentualConcluido = i.QuantidadePlanejada > 0
                    ? Math.Round(i.QuantidadeProduzida * 100m / i.QuantidadePlanejada)
                    : 0m
            });

        var paginado = await query.AplicarBuscaAsync(
            req,
            mapaColunas,
            colunasBuscaGlobal: ["ordemProducaoCodigo", "produtoCodigo", "produtoDescricao"]);

        return new PaginadoResponse<DemandaItemDTO>
        {
            Itens = paginado.Itens.Select(ToDemandaItemDTO).ToList(),
            Total = paginado.Total,
            TotalGeral = paginado.TotalGeral,
            Pagina = paginado.Pagina,
            Tamanho = paginado.Tamanho,
            TotalPaginas = paginado.TotalPaginas
        };
    }

    private static DemandaItemDTO ToDemandaItemDTO(DemandaQueryProj p)
    {
        var i = p.Item;
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
            QuantidadeFaltante = p.QuantidadeFaltante,
            PercentualConcluido = Math.Round(p.PercentualConcluido, 2)
        };
    }
}
