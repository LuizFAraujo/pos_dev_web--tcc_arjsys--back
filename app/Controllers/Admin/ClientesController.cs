using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Clientes")]
public class ClientesController(ClienteService service) : ControllerBase
{
    private readonly ClienteService _service = service;

    /// <summary>
    /// Lista clientes. Suporta filtro por texto em nome, código, CPF/CNPJ e cidade.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ClienteResponseDTO>>> GetAll([FromQuery] string? busca = null)
    {
        return await _service.GetAll(busca);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponseDTO>> GetById(int id)
    {
        var cliente = await _service.GetById(id);

        if (cliente == null)
            return NotFound();

        return cliente;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponseDTO>> Create(ClienteCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClienteCreateDTO dto)
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
