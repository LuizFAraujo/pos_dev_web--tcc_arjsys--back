using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
[Tags("Engenharia - Grupo Vínculos")]
public class GrupoVinculoController(GrupoVinculoService service) : ControllerBase
{
    private readonly GrupoVinculoService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<GrupoVinculoResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("pai/{paiId:int}")]
    public async Task<ActionResult<List<GrupoVinculoResponseDTO>>> GetByPaiId(int paiId)
    {
        return await _service.GetByPaiId(paiId);
    }

    [HttpPost]
    public async Task<ActionResult<GrupoVinculoResponseDTO>> Create(GrupoVinculoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (sucesso, erro) = await _service.Delete(id);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}