using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Repositories;
using AgroControl.Api.Security;

namespace AgroControl.Api.Services;

public interface IUsuarioService
{
    Task<(bool Success, string Reply)> CriarAsync(CriarUsuarioDto dto);
    Task<(bool Success, string Reply)> ExcluirAsync(long id);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository) => _repository = repository;

    public async Task<(bool Success, string Reply)> CriarAsync(CriarUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return (false, "Nome, e-mail e senha sao obrigatorios.");

        // Anti-XSS / payload corrompido em campos textuais
        if (InputSanitizer.ContemConteudoSuspeito(dto.Nome) || InputSanitizer.ContemConteudoSuspeito(dto.Email))
            return (false, "Entrada invalida: caracteres nao permitidos detectados.");

        // Hash da senha com BCrypt antes de persistir
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var (ok, reply) = await _repository.CriarAsync(dto.Nome.Trim(), dto.Email.Trim().ToLowerInvariant(), senhaHash);
        if (!ok)
            return (false, reply);

        return (true, $"Usuario criado com sucesso. Id={reply}");
    }

    public async Task<(bool Success, string Reply)> ExcluirAsync(long id)
    {
        if (id <= 0)
            return (false, "Id invalido.");

        return await _repository.ExcluirAsync(id);
    }
}
