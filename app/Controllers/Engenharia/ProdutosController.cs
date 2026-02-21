using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class ProdutosController(ProdutoService service) : ControllerBase
{
    private readonly ProdutoService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ProdutoResponseDTO>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDTO>> GetById(int id)
    {
        var produto = await _service.GetById(id);

        if (produto == null)
            return NotFound();

        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponseDTO>> Create(ProdutoCreateDTO dto)
    {
        var criado = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProdutoCreateDTO dto)
    {
        var atualizado = await _service.Update(id, dto);

        if (!atualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _service.Delete(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}