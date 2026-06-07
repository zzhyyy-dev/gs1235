using Dapper;
using AgroControl.Api.Data;

namespace AgroControl.Api.Repositories;

public interface ITelemetriaRepository
{
    Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, double temperatura, double umidade, double agua, double luminosidade);
    Task<(bool Success, string Reply)> ObterRecentePorEstufaAsync(long idEstufa);
    Task<(bool Success, string Reply)> LimparAntigosAsync(int dias);
}

public class TelemetriaRepository : ITelemetriaRepository
{
    private readonly IDbConnectionFactory _factory;

    public TelemetriaRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, double temperatura, double umidade, double agua, double luminosidade)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var novoId = await conn.ExecuteScalarAsync<long>(
                @"INSERT INTO T_AGC_TELEMETRIA_LOG
                      (ID_DISPOSITIVO, VL_TEMPERATURA, VL_UMIDADE, VL_AGUA, VL_LUMINOSIDADE)
                  VALUES
                      (@IdDispositivo, @Temperatura, @Umidade, @Agua, @Luminosidade);
                  SELECT last_insert_rowid();",
                new
                {
                    IdDispositivo = idDispositivo,
                    Temperatura = temperatura,
                    Umidade = umidade,
                    Agua = agua,
                    Luminosidade = luminosidade
                });

            return (true, novoId.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao inserir telemetria: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> ObterRecentePorEstufaAsync(long idEstufa)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var leitura = await conn.QueryFirstOrDefaultAsync(
                @"SELECT  t.ID_TELEMETRIA   AS idTelemetria,
                          t.ID_DISPOSITIVO  AS idDispositivo,
                          d.CD_UUID         AS codigoUuid,
                          t.VL_TEMPERATURA  AS temperatura,
                          t.VL_UMIDADE      AS umidade,
                          t.VL_AGUA         AS agua,
                          t.VL_LUMINOSIDADE AS luminosidade,
                          t.DT_TIMESTAMP    AS timestamp
                  FROM T_AGC_TELEMETRIA_LOG t
                  INNER JOIN T_AGC_DISPOSITIVO d ON d.ID_DISPOSITIVO = t.ID_DISPOSITIVO
                  WHERE d.ID_ESTUFA = @IdEstufa
                  ORDER BY t.DT_TIMESTAMP DESC, t.ID_TELEMETRIA DESC
                  LIMIT 1",
                new { IdEstufa = idEstufa });

            if (leitura is null)
                return (false, "Nenhuma leitura encontrada para a estufa informada.");

            return (true, Json.Serialize(leitura));
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao buscar telemetria recente: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> LimparAntigosAsync(int dias)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            // Parametro tipado: usa o modificador de data nativo do SQLite
            var linhas = await conn.ExecuteAsync(
                @"DELETE FROM T_AGC_TELEMETRIA_LOG
                  WHERE DT_TIMESTAMP < datetime('now', @Modificador)",
                new { Modificador = $"-{dias} days" });

            return (true, $"{linhas} registro(s) de telemetria removido(s) (anteriores a {dias} dia(s)).");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao limpar telemetria antiga: {ex.Message}");
        }
    }
}
