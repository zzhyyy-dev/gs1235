using AgroControl.Api.Models;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Repositories;

namespace AgroControl.Api.Services;

public interface IAuthService
{
    Task<(bool Success, string Reply)> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;

    public AuthService(IUsuarioRepository repository) => _repository = repository;

    public async Task<(bool Success, string Reply)> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return (false, "E-mail e senha sao obrigatorios.");

        var (ok, reply) = await _repository.ObterPorEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // Mensagem generica evita enumeracao de usuarios
        if (!ok)
            return (false, "Credenciais invalidas.");

        var usuario = Json.Deserialize<Usuario>(reply);
        if (usuario is null)
            return (false, "Credenciais invalidas.");

        // Verify compara a senha em claro com o hash BCrypt armazenado
        var senhaConfere = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
        if (!senhaConfere)
            return (false, "Credenciais invalidas.");

        return (true, $"Login efetuado com sucesso. Bem-vindo(a), {usuario.Nome}.");
    }
}
