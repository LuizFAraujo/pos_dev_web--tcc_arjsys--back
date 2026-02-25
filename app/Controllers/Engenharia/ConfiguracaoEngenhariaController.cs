using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
[Tags("Engenharia - Configurações")]
public class ConfiguracaoEngenhariaController(ConfiguracaoEngenhariaService service) : ControllerBase
{
    private readonly ConfiguracaoEngenhariaService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ConfiguracaoEngenhariaResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{chave}")]
    public async Task<ActionResult<ConfiguracaoEngenhariaResponseDTO>> GetByChave(string chave)
    {
        var config = await _service.GetByChave(chave);

        if (config == null)
            return NotFound();

        return config;
    }

    [HttpPost]
    public async Task<ActionResult<ConfiguracaoEngenhariaResponseDTO>> Create(ConfiguracaoEngenhariaCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ConfiguracaoEngenhariaCreateDTO dto)
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