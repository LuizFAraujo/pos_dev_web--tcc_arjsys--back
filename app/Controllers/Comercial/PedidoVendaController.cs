using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Comercial;

namespace Api_ArjSys_Tcc.Controllers.Comercial;

[ApiController]
[Route("api/comercial/[controller]")]
[Tags("Comercial - Pedidos de Venda")]
public class PedidoVendaController(PedidoVendaService service) : ControllerBase
{
    private readonly PedidoVendaService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PedidoVendaResponseDTO>>> GetAll(int pagina = 0, int tamanho = 0)
    {
        return await _service.GetAll(pagina, tamanho);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoVendaResponseDTO>> GetById(int id)
    {
        var pedido = await _service.GetById(id);

        if (pedido == null)
            return NotFound();

        return pedido;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoVendaResponseDTO>> Create(PedidoVendaCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PedidoVendaCreateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, StatusPedidoVendaDTO dto)
    {
        var (sucesso, erro) = await _service.AlterarStatus(id, dto);

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