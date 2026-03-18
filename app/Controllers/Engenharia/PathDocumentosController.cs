using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
[Tags("Engenharia - Path Documentos")]
public class PathDocumentosController(PathDocumentosService service) : ControllerBase
{
    private readonly PathDocumentosService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PathDocumentosResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PathDocumentosResponseDTO>> GetById(int id)
    {
        var item = await _service.GetById(id);

        if (item == null)
            return NotFound();

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<PathDocumentosResponseDTO>> Create(PathDocumentosCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PathDocumentosUpdateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
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