using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Configuração da Empresa")]
public class ConfiguracaoEmpresaController(ConfiguracaoEmpresaService service) : ControllerBase
{
    private readonly ConfiguracaoEmpresaService _service = service;

    /// <summary>
    /// Retorna a configuração singleton da empresa (Id = 1, AnoFundacao, Configurado).
    /// Se ainda não existir, cria automaticamente com AnoFundacao = ano atual e Configurado = false.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ConfiguracaoEmpresaResponseDTO>> Get()
    {
        var config = await _service.Get();
        return Ok(config);
    }

    /// <summary>
    /// Atualiza o AnoFundacao (uso comum).
    /// Bloqueia se já houver NS emitido - nesse caso, exige uso do endpoint admin-override.
    /// Marca Configurado = true (libera emissão de NS).
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update(ConfiguracaoEmpresaUpdateDTO dto)
    {
        var (sucesso, erro) = await _service.Update(dto);

        if (!sucesso)
            return BadRequest(new { erro });

        return NoContent();
    }

    /// <summary>
    /// Atualiza o AnoFundacao mesmo que já existam NS no banco (admin-override).
    /// NS já gerados NÃO são reescritos - ficam com o código original.
    /// Reservado pra role Admin. Autenticação ainda não implementada.
    /// </summary>
    // TODO: ativar quando implementar autenticação/roles
    // [Authorize(Roles = "Admin")]
    [HttpPut("admin-override")]
    public async Task<IActionResult> UpdateAdmin(ConfiguracaoEmpresaUpdateDTO dto)
    {
        var (sucesso, erro) = await _service.UpdateAdmin(dto);

        if (!sucesso)
            return BadRequest(new { erro });

        return NoContent();
    }
}
