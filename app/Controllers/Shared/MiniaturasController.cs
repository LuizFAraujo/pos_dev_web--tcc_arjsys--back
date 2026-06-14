using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.Services.Shared.Thumbnail;

namespace Api_ArjSys_Tcc.Controllers.Shared;

/// <summary>
/// Manutenção do cache de miniaturas. É uma ferramenta genérica e transversal
/// (não pertence a nenhum módulo de negócio), por isso fica em Controllers/Shared,
/// junto das demais ferramentas compartilhadas. Rota api/[controller].
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Shared - Miniaturas")]
public class MiniaturasController(ThumbnailService thumbnailService) : ControllerBase
{
    private readonly ThumbnailService _thumbnailService = thumbnailService;

    /// <summary>
    /// Limpa o cache de miniaturas em disco.
    /// Sem parâmetros apaga tudo. Com 'dias' apaga as geradas há mais de N dias.
    /// Com 'ate' apaga as geradas antes dessa data. Informe apenas um dos dois.
    /// </summary>
    [HttpDelete("cache")]
    public IActionResult LimparCache([FromQuery] int? dias, [FromQuery] DateTime? ate)
    {
        if (dias.HasValue && ate.HasValue)
            return BadRequest(new { erro = "Informe apenas 'dias' ou 'ate', não os dois." });

        if (dias is < 0)
            return BadRequest(new { erro = "'dias' não pode ser negativo." });

        var removidas = _thumbnailService.LimparCache(dias, ate);

        return Ok(new { removidas, dias, ate });
    }
}
