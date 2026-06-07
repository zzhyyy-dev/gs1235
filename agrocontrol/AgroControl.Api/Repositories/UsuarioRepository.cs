using Dapper;
using AgroControl.Api.Data;
using AgroControl.Api.Models;

namespace AgroControl.Api.Repositories;

public interface IUsuarioRepository
{
    Task<(bool Success, string Reply)> CriarAsync(string nome, string email, string senhaHash);
    Task<(bool Success, string Reply)> ObterPorEmailAsync(string email);
    Task<(bool Success, string Reply)> ExcluirAsync(long id);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _factory;

    public UsuarioRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<(bool Success, string Reply)> CriarAsync(string nome, string email, string senhaHash)
    {
        try
        {
            using var conn = _factory.CreateConnection();

            // Consulta parametrizada (objeto anonimo) -> Anti-SQLi
            var jaExiste = await conn.ExecuteScalarAsync<long>(
                "SELECT COUNT(1) FROM T_AGC_USUARIO WHERE DS_EMAIL = @Email",
                new { Email = email });

            if (jaExiste > 0)
                return (false, "E-mail ja cadastrado.");

            var novoId = await conn.ExecuteScalarAsync<long>(
                @"INSERT INTO T_AGC_USUARIO (NM_USUARIO, DS_EMAIL, DS_SENHA_HASH)
                  VALUES (@Nome, @Email, @SenhaHash);
                  SELECT last_insert_rowid();",
                new { Nome = nome, Email = email, SenhaHash = senhaHash });

            return (true, novoId.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao criar usuario: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> ObterPorEmailAsync(string email)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var usuario = await conn.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT ID_USUARIO AS IdUsuario, NM_USUARIO AS Nome, DS_EMAIL AS Email,
                         DS_SENHA_HASH AS SenhaHash, DT_CRIACAO AS DataCriacao
                  FROM T_AGC_USUARIO
                  WHERE DS_EMAIL = @Email",
                new { Email = email });

            if (usuario is null)
                return (false, "Usuario nao encontrado.");

            return (true, Json.Serialize(usuario));
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao buscar usuario: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> ExcluirAsync(long id)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var linhas = await conn.ExecuteAsync(
                "DELETE FROM T_AGC_USUARIO WHERE ID_USUARIO = @Id",
                new { Id = id });

            return linhas > 0
                ? (true, "Usuario removido com sucesso.")
                : (false, "Usuario nao encontrado.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao remover usuario: {ex.Message}");
        }
    }
}
