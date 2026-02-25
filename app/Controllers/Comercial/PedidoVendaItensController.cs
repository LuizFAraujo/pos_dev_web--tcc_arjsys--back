using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Comercial;

namespace Api_ArjSys_Tcc.Controllers.Comercial;

[ApiController]
[Route("api/comercial/PedidoVenda/{pedidoId:int}/itens")]
[Tags("Comercial - Itens do Pedido")]
public class PedidoVendaItensController(PedidoVendaItemService service) : ControllerBase
{
    private readonly PedidoVendaItemService _service = service;

    [HttpGet]
    public async Task<ActionResult<List<PedidoVendaItemResponseDTO>>> GetByPedidoId(int pedidoId)
    {
        return await _service.GetByPedidoId(pedidoId);
    }

    [HttpPost]
    public async Task<ActionResult<PedidoVendaItemResponseDTO>> Create(int pedidoId, PedidoVendaItemCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(pedidoId, dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int pedidoId, int id, PedidoVendaItemCreateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(pedidoId, id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int pedidoId, int id)
    {
        var (sucesso, erro) = await _service.Delete(pedidoId, id);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}