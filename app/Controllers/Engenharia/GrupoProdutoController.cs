using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class GrupoProdutoController(GrupoProdutoService service) : ControllerBase
{
    private readonly GrupoProdutoService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<GrupoProdutoResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("nivel/{nivel}")]
    public async Task<ActionResult<List<GrupoProdutoResponseDTO>>> GetByNivel(string nivel)
    {
        return await _service.GetByNivel(nivel);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GrupoProdutoResponseDTO>> GetById(int id)
    {
        var grupo = await _service.GetById(id);

        if (grupo == null)
            return NotFound();

        return grupo;
    }

    [HttpPost]
    public async Task<ActionResult<GrupoProdutoResponseDTO>> Create(GrupoProdutoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GrupoProdutoCreateDTO dto)
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