using Microsoft.AspNetCore.Mvc;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Services;

namespace AgroControl.Api.Controllers;

[ApiController]
[Route("api/telemetria")]
public class TelemetriaController : ControllerBase
{
    private readonly ITelemetriaService _service;

    public TelemetriaController(ITelemetriaService service) => _service = service;

    // POST /api/telemetria
    [HttpPost]
    public async Task<IActionResult> Receber([FromBody] TelemetriaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { sucesso = false, mensagem = "Payload de telemetria invalido." });

        var (ok, reply) = await _service.RegistrarAsync(dto);
        if (!ok)
            return BadRequest(new { sucesso = false, mensagem = reply });

        // reply = JSON com a telemetria persistida e os alertas gerados
        return new ContentResult
        {
            Content = reply,
            ContentType = "application/json",
            StatusCode = StatusCodes.Status201Created
        };
    }

    // GET /api/telemetria/recente/{id}  (id = estufa)
    [HttpGet("recente/{id:long}")]
    public async Task<IActionResult> Recente(long id)
    {
        var (ok, reply) = await _service.ObterRecentePorEstufaAsync(id);
        if (!ok)
            return NotFound(new { sucesso = false, mensagem = reply });

        return Content(reply, "application/json");
    }

    // DELETE /api/telemetria/limpar-antigos?dias=30
    [HttpDelete("limpar-antigos")]
    public async Task<IActionResult> LimparAntigos([FromQuery] int dias = 30)
    {
        var (ok, reply) = await _service.LimparAntigosAsync(dias);
        if (!ok)
            return BadRequest(new { sucesso = false, mensagem = reply });

        return Ok(new { sucesso = true, mensagem = reply });
    }
}
