using Microsoft.AspNetCore.Mvc;

namespace Api_ArjSys_Tcc.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Sistema - Status")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "OK",
            Aplicacao = "ARJSYS API",
            Versao = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }
}