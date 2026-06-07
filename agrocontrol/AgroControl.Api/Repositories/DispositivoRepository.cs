using Dapper;
using AgroControl.Api.Data;
using AgroControl.Api.Models;

namespace AgroControl.Api.Repositories;

public interface IDispositivoRepository
{
    Task<(bool Success, string Reply)> ObterPorUuidAsync(string uuid);
}

public class DispositivoRepository : IDispositivoRepository
{
    private readonly IDbConnectionFactory _factory;

    public DispositivoRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<(bool Success, string Reply)> ObterPorUuidAsync(string uuid)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var disp = await conn.QueryFirstOrDefaultAsync<Dispositivo>(
                @"SELECT ID_DISPOSITIVO AS IdDispositivo, CD_UUID AS CodigoUuid, ID_ESTUFA AS IdEstufa
                  FROM T_AGC_DISPOSITIVO
                  WHERE CD_UUID = @Uuid",
                new { Uuid = uuid });

            if (disp is null)
                return (false, "Dispositivo nao encontrado.");

            return (true, Json.Serialize(disp));
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao buscar dispositivo: {ex.Message}");
        }
    }
}
