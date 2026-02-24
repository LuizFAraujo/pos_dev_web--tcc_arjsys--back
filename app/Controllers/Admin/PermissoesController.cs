using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Permissões")]
public class PermissoesController(PermissaoService service) : ControllerBase
{
    private readonly PermissaoService _service = service;

    [HttpGet("funcionario/{funcionarioId:int}")]
    public async Task<ActionResult<List<PermissaoResponseDTO>>> GetByFuncionarioId(int funcionarioId)
    {
        return await _service.GetByFuncionarioId(funcionarioId);
    }

    [HttpPost]
    public async Task<ActionResult<PermissaoResponseDTO>> Create(PermissaoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PermissaoCreateDTO dto)
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