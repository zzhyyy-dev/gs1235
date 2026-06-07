using Dapper;
using AgroControl.Api.Data;

namespace AgroControl.Api.Repositories;

public interface IAlertaRepository
{
    Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, string tipo, string mensagem, string gravidade);
    Task<(bool Success, string Reply)> ListarCriticosAtivosAsync();
    Task<(bool Success, string Reply)> ResolverAsync(long idAlerta);
}

public class AlertaRepository : IAlertaRepository
{
    private readonly IDbConnectionFactory _factory;

    public AlertaRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, string tipo, string mensagem, string gravidade)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var novoId = await conn.ExecuteScalarAsync<long>(
                @"INSERT INTO T_AGC_ALERTA_EVENTO
                      (ID_DISPOSITIVO, TP_ALERTA, DS_MENSAGEM, DS_GRAVIDADE, ST_RESOLVIDO)
                  VALUES
                      (@IdDispositivo, @Tipo, @Mensagem, @Gravidade, 'N');
                  SELECT last_insert_rowid();",
                new { IdDispositivo = idDispositivo, Tipo = tipo, Mensagem = mensagem, Gravidade = gravidade });

            return (true, novoId.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao registrar alerta: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> ListarCriticosAtivosAsync()
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var alertas = await conn.QueryAsync(
                @"SELECT  a.ID_ALERTA       AS idAlerta,
                          a.ID_DISPOSITIVO  AS idDispositivo,
                          d.CD_UUID         AS codigoUuid,
                          a.TP_ALERTA       AS tipo,
                          a.DS_MENSAGEM     AS mensagem,
                          a.DS_GRAVIDADE    AS gravidade,
                          a.DT_TIMESTAMP    AS timestamp,
                          a.ST_RESOLVIDO    AS stResolvido
                  FROM T_AGC_ALERTA_EVENTO a
                  INNER JOIN T_AGC_DISPOSITIVO d ON d.ID_DISPOSITIVO = a.ID_DISPOSITIVO
                  WHERE a.DS_GRAVIDADE = 'Critico' AND a.ST_RESOLVIDO = 'N'
                  ORDER BY a.DT_TIMESTAMP DESC, a.ID_ALERTA DESC");

            return (true, Json.Serialize(alertas));
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao listar alertas criticos: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Reply)> ResolverAsync(long idAlerta)
    {
        try
        {
            using var conn = _factory.CreateConnection();
            var linhas = await conn.ExecuteAsync(
                @"UPDATE T_AGC_ALERTA_EVENTO
                  SET ST_RESOLVIDO = 'S'
                  WHERE ID_ALERTA = @Id",
                new { Id = idAlerta });

            return linhas > 0
                ? (true, "Alerta marcado como resolvido (contingencia aplicada).")
                : (false, "Alerta nao encontrado.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao resolver alerta: {ex.Message}");
        }
    }
}
