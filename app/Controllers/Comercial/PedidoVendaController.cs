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

    /// <summary>
    /// Lista todos os PVs. Suporta paginação opcional.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PedidoVendaResponseDTO>>> GetAll(int pagina = 0, int tamanho = 0)
    {
        return await _service.GetAll(pagina, tamanho);
    }

    /// <summary>
    /// Busca PV por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoVendaResponseDTO>> GetById(int id)
    {
        var pedido = await _service.GetById(id);

        if (pedido == null)
            return NotFound();

        return pedido;
    }

    /// <summary>
    /// Cria Pedido de Venda com itens em chamada única (atômica).
    /// Tipo obrigatório (Normal ou PreVenda). Lista de itens obrigatória com pelo menos 1 item.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoVendaResponseDTO>> Create(PedidoVendaCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    /// <summary>
    /// Atualiza PV + itens em chamada única com diff sincronizado (atômica).
    /// Itens com Id são atualizados; sem Id são criados; ausentes da lista são removidos.
    /// Em status avançado (Andamento/Concluido/AEntregar/Pausado) justificativa é obrigatória,
    /// registra evento ItensAlterados e notifica Engenharia/Produção/Almoxarifado.
    /// Retorna 200 OK com o PV completo atualizado.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PedidoVendaResponseDTO>> Update(int id, PedidoVendaUpdateDTO dto)
    {
        var (atualizado, erro) = await _service.Update(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (atualizado == null)
            return NotFound();

        return Ok(atualizado);
    }

    /// <summary>
    /// Altera status do PV. Justificativa é obrigatória em Pausar, Cancelar, Reabrir, Devolver e retroceder.
    /// </summary>
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

    /// <summary>
    /// Exclui PV. Permitido apenas em AguardandoNS ou Liberado, sem NS e sem OPs vinculadas.
    /// </summary>
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

    /// <summary>
    /// Retorna o histórico de eventos do Pedido de Venda (mais recente primeiro).
    /// </summary>
    [HttpGet("{id:int}/historico")]
    public async Task<ActionResult<List<PedidoVendaHistoricoResponseDTO>>> GetHistorico(int id)
    {
        return await _service.GetHistorico(id);
    }
}
