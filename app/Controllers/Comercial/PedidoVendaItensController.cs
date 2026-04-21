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

    /// <summary>
    /// Lista os itens de um PV.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PedidoVendaItemResponseDTO>>> GetByPedidoId(int pedidoId)
    {
        return await _service.GetByPedidoId(pedidoId);
    }

    /// <summary>
    /// Adiciona item ao PV.
    /// - Status iniciais (AguardandoNS/RecebidoNS/AguardandoRetorno/Liberado): livre.
    /// - Status avançados (Andamento/Concluido/AEntregar/Pausado): justificativa obrigatória no body.
    /// - Status terminais: bloqueado.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoVendaItemResponseDTO>> Create(int pedidoId, PedidoVendaItemCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(pedidoId, dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    /// <summary>
    /// Atualiza item do PV.
    /// Mesma regra do POST: status iniciais livre, avançados exigem justificativa no body, terminais bloqueados.
    /// </summary>
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

    /// <summary>
    /// Remove item do PV.
    /// Em status avançado, justificativa é obrigatória (via query param ?justificativa=...).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int pedidoId, int id, [FromQuery] string? justificativa = null)
    {
        var (sucesso, erro) = await _service.Delete(pedidoId, id, justificativa);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}
