using Microsoft.AspNetCore.Mvc;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Services;

namespace AgroControl.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    // POST /api/usuarios/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CriarUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { sucesso = false, mensagem = "Dados invalidos.", erros = ModelState });

        var (ok, reply) = await _service.CriarAsync(dto);
        if (!ok)
            return BadRequest(new { sucesso = false, mensagem = reply });

        return StatusCode(StatusCodes.Status201Created, new { sucesso = true, mensagem = reply });
    }

    // DELETE /api/usuarios/delete/{id}
    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var (ok, reply) = await _service.ExcluirAsync(id);
        if (!ok)
        {
            return reply.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { sucesso = false, mensagem = reply })
                : BadRequest(new { sucesso = false, mensagem = reply });
        }

        return Ok(new { sucesso = true, mensagem = reply });
    }
}
