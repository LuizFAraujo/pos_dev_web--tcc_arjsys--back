using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Notificações")]
public class NotificacoesController(NotificacaoService service) : ControllerBase
{
    private readonly NotificacaoService _service = service;

    /// <summary>
    /// Lista notificações de um módulo. Filtro opcional por lidas/não lidas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NotificacaoResponseDTO>>> GetByModulo(
        [FromQuery] ModuloSistema modulo,
        [FromQuery] bool? lidas = null,
        [FromQuery] int pagina = 0,
        [FromQuery] int tamanho = 0)
    {
        return await _service.GetByModulo(modulo, lidas, pagina, tamanho);
    }

    /// <summary>
    /// Busca notificação por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificacaoResponseDTO>> GetById(int id)
    {
        var n = await _service.GetById(id);

        if (n == null)
            return NotFound();

        return n;
    }

    /// <summary>
    /// Retorna a contagem de notificações não lidas de um módulo.
    /// </summary>
    [HttpGet("nao-lidas/contagem")]
    public async Task<ActionResult<int>> ContarNaoLidas([FromQuery] ModuloSistema modulo)
    {
        return await _service.ContarNaoLidas(modulo);
    }

    /// <summary>
    /// Cria uma notificação.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificacaoResponseDTO>> Create(NotificacaoCreateDTO dto)
    {
        var (criado, erro) = await _service.Create(dto);

        if (erro != null)
            return BadRequest(new { erro });

        return Ok(criado);
    }

    /// <summary>
    /// Marca uma notificação como lida.
    /// </summary>
    [HttpPatch("{id:int}/lida")]
    public async Task<IActionResult> MarcarLida(int id)
    {
        var (sucesso, erro) = await _service.MarcarLida(id);

        if (erro != null)
            return BadRequest(new { erro });

        if (!sucesso)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Marca todas as notificações não lidas de um módulo como lidas.
    /// </summary>
    [HttpPatch("modulo/{modulo}/marcar-todas-lidas")]
    public async Task<ActionResult<object>> MarcarTodasLidas(ModuloSistema modulo)
    {
        var quantidade = await _service.MarcarTodasLidas(modulo);
        return Ok(new { afetadas = quantidade });
    }

    /// <summary>
    /// Exclui uma notificação.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sucesso = await _service.Delete(id);

        if (!sucesso)
            return NotFound();

        return NoContent();
    }
}
