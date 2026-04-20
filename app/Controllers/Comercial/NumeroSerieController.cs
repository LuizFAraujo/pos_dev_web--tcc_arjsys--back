using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Comercial;

namespace Api_ArjSys_Tcc.Controllers.Comercial;

[ApiController]
[Route("api/comercial/[controller]")]
[Tags("Comercial - Número de Série")]
public class NumeroSerieController(NumeroSerieService service) : ControllerBase
{
    private readonly NumeroSerieService _service = service;

    /// <summary>
    /// Lista todos os NS com dados do PV vinculado. Suporta paginação.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NumeroSerieResponseDTO>>> GetAll(int pagina = 0, int tamanho = 0)
    {
        return await _service.GetAll(pagina, tamanho);
    }

    /// <summary>
    /// Busca NS por ID com dados do PV vinculado.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NumeroSerieResponseDTO>> GetById(int id)
    {
        var ns = await _service.GetById(id);

        if (ns == null)
            return NotFound();

        return ns;
    }

    /// <summary>
    /// Busca o NS vinculado a um PV (relação 1:1). Retorna 404 se o PV não tiver NS.
    /// </summary>
    [HttpGet("pedido/{pedidoId:int}")]
    public async Task<ActionResult<NumeroSerieResponseDTO>> GetByPedidoId(int pedidoId)
    {
        var ns = await _service.GetByPedidoId(pedidoId);

        if (ns == null)
            return NotFound();

        return ns;
    }

    /// <summary>
    /// Cria NS vinculado a um PV. Regras: PV em Aguardando/EmAndamento, não Cancelado, sem NS prévio.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NumeroSerieResponseDTO>> Create(NumeroSerieCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return CreatedAtAction(nameof(GetById), new { id = criado!.Id }, criado);
    }

    /// <summary>
    /// Atualiza dados do NS (apenas campos próprios do NS — PV é readonly aqui).
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, NumeroSerieUpdateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(id, dto);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}
