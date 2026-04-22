using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Funcionários")]
public class FuncionariosController(FuncionarioService service) : ControllerBase
{
    private readonly FuncionarioService _service = service;

    /// <summary>
    /// Lista funcionários. Suporta filtro por texto em nome, código, usuário e cargo.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FuncionarioResponseDTO>>> GetAll([FromQuery] string? busca = null)
    {
        return await _service.GetAll(busca);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FuncionarioResponseDTO>> GetById(int id)
    {
        var func = await _service.GetById(id);

        if (func == null)
            return NotFound();

        return func;
    }

    [HttpPost]
    public async Task<ActionResult<FuncionarioResponseDTO>> Create(FuncionarioCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FuncionarioCreateDTO dto)
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
