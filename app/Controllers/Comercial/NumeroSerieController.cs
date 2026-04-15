using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;

namespace Api_ArjSys_Tcc.Controllers.Comercial;

[ApiController]
[Route("api/comercial/[controller]")]
[Tags("Comercial - Número de Série")]
public class NumeroSerieController(NumeroSerieService service) : ControllerBase
{
    private readonly NumeroSerieService _service = service;

    /// <summary>
    /// Listar todos os NS. Suporta paginação e filtro por tipo (?tipo=Normal ou ?tipo=VendaFutura).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NumeroSerieResponseDTO>>> GetAll(
        int pagina = 0,
        int tamanho = 0,
        [FromQuery] TipoNumeroSerie? tipo = null)
    {
        return await _service.GetAll(pagina, tamanho, tipo);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NumeroSerieResponseDTO>> GetById(int id)
    {
        var ns = await _service.GetById(id);

        if (ns == null)
            return NotFound();

        return ns;
    }

    [HttpGet("pedido/{pedidoId:int}")]
    public async Task<ActionResult<List<NumeroSerieResponseDTO>>> GetByPedidoId(int pedidoId)
    {
        return await _service.GetByPedidoId(pedidoId);
    }

    [HttpPost]
    public async Task<ActionResult<NumeroSerieResponseDTO>> Create(NumeroSerieCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, StatusNumeroSerieDTO dto)
    {
        var (sucesso, erro) = await _service.AlterarStatus(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}