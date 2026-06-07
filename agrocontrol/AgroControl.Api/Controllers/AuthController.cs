using Microsoft.AspNetCore.Mvc;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Services;

namespace AgroControl.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { sucesso = false, mensagem = "Dados invalidos." });

        var (ok, reply) = await _service.LoginAsync(dto);
        if (!ok)
            return Unauthorized(new { sucesso = false, mensagem = reply });

        return Ok(new { sucesso = true, mensagem = reply });
    }
}
