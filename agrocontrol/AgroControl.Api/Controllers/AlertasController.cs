using Microsoft.AspNetCore.Mvc;
using AgroControl.Api.Services;

namespace AgroControl.Api.Controllers;

[ApiController]
[Route("api/alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _service;

    public AlertasController(IAlertaService service) => _service = service;

    // GET /api/alertas/criticos
    [HttpGet("criticos")]
    public async Task<IActionResult> Criticos()
    {
        var (ok, reply) = await _service.ListarCriticosAsync();
        if (!ok)
            return BadRequest(new { sucesso = false, mensagem = reply });

        return Content(reply, "application/json");
    }

    // PUT /api/alertas/{id}/resolver
    [HttpPut("{id:long}/resolver")]
    public async Task<IActionResult> Resolver(long id)
    {
        var (ok, reply) = await _service.ResolverAsync(id);
        if (!ok)
        {
            return reply.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { sucesso = false, mensagem = reply })
                : BadRequest(new { sucesso = false, mensagem = reply });
        }

        return Ok(new { sucesso = true, mensagem = reply });
    }
}
