using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Engenharia;
using Api_ArjSys_Tcc.Services.Engenharia;

namespace Api_ArjSys_Tcc.Controllers.Engenharia;

[ApiController]
[Route("api/engenharia/[controller]")]
public class BomController(BomService service) : ControllerBase
{
    private readonly BomService _service = service;

    [HttpGet]
    public async Task<ActionResult> GetProdutosComEstrutura(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 0)
    {
        var (itens, total) = await _service.GetProdutosComEstrutura(pagina, tamanho);

        return Ok(new
        {
            itens,
            total,
            pagina,
            tamanho,
            totalPaginas = tamanho > 0 ? (int)Math.Ceiling((double)total / tamanho) : 1
        });
    }

    [HttpGet("produto/{produtoPaiId:int}")]
    public async Task<ActionResult<List<EstruturaProdutoResponseDTO>>> GetByProdutoId(int produtoPaiId)
    {
        return await _service.GetByProdutoId(produtoPaiId);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstruturaProdutoResponseDTO>> GetById(int id)
    {
        var item = await _service.GetById(id);

        if (item == null)
            return NotFound();

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<EstruturaProdutoResponseDTO>> Create(EstruturaProdutoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EstruturaProdutoCreateDTO dto)
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
        var removido = await _service.Delete(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }

    [HttpGet("flat")]
    public async Task<ActionResult> GetAllFlat(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 0)
    {
        var (itens, total) = await _service.GetAllFlat(pagina, tamanho);

        return Ok(new
        {
            itens,
            total,
            pagina,
            tamanho,
            totalPaginas = tamanho > 0 ? (int)Math.Ceiling((double)total / tamanho) : 1
        });
    }


}