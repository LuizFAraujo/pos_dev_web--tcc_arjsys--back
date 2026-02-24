using Microsoft.AspNetCore.Mvc;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Tags("Admin - Auth")]
public class AuthController(AuthService service) : ControllerBase
{
    private readonly AuthService _service = service;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
    {
        var (dados, erro) = await _service.Login(dto);

        if (erro != null)
            return Unauthorized(new { erro });

        return Ok(dados);
    }
}