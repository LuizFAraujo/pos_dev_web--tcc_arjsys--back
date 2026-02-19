using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class ProdutosController(ProdutoService service) : ControllerBase
{
    private readonly ProdutoService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<Produto>>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetById(int id)
    {
        var produto = await _service.GetById(id);

        if (produto == null)
            return NotFound();

        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Create(Produto produto)
    {
        var criado = await _service.Create(produto);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Produto produto)
    {
        if (id != produto.Id)
            return BadRequest();

        var atualizado = await _service.Update(id, produto);

        if (!atualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _service.Delete(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}