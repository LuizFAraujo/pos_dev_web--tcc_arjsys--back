using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Producao;
using Api_ArjSys_Tcc.DTOs.Shared;
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
    /// Busca paginada da demanda com filtros, ordenação e busca textual server-side.
    /// </summary>
    [HttpPost("buscar")]
    public async Task<ActionResult<PaginadoResponse<DemandaItemDTO>>> Buscar([FromBody] BuscaRequest req)
    {
        var resposta = await _service.Buscar(req);
        return Ok(resposta);
    }
}
