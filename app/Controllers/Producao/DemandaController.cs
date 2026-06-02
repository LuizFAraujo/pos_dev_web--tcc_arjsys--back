using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Producao;
using Api_ArjSys_Tcc.DTOs.Shared;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.Services.Producao;

namespace Api_ArjSys_Tcc.Controllers.Producao;

/// <summary>
/// Demanda de Produção - visão agregada de itens em OPs ativas.
/// </summary>
[ApiController]
[Route("api/producao/[controller]")]
[Tags("Produção - Demanda")]
public class DemandaController(DemandaService service) : ControllerBase
{
    private readonly DemandaService _service = service;

    /// <summary>
    /// Lista a demanda agregada por produto. Filtro opcional por tipos (CSV).
    /// Exemplos:
    ///   GET /api/producao/Demanda                              → todos os tipos
    ///   GET /api/producao/Demanda?tipos=Fabricado              → A Fabricar
    ///   GET /api/producao/Demanda?tipos=Comprado,MateriaPrima  → A Comprar
    /// Tipos inválidos no CSV são ignorados silenciosamente.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DemandaItemDTO>>> Listar(string? tipos = null)
    {
        List<TipoProduto>? lista = null;
        if (!string.IsNullOrWhiteSpace(tipos))
        {
            lista = tipos
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => Enum.TryParse<TipoProduto>(t, true, out var v) ? v : (TipoProduto?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
        }

        return await _service.Listar(lista);
    }

    /// <summary>
    /// Busca paginada da demanda com filtros, ordenação e busca textual server-side.
    /// </summary>
    [HttpPost("buscar")]
    public async Task<ActionResult<PaginadoResponse<DemandaItemDTO>>> Buscar([FromBody] BuscaRequest req)
    {
        var resposta = await _service.Buscar(req);
        return Ok(resposta);
    }
}
