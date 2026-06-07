using AgroControl.Api.Models;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Repositories;
using AgroControl.Api.Security;

namespace AgroControl.Api.Services;

public interface ITelemetriaService
{
    Task<(bool Success, string Reply)> RegistrarAsync(TelemetriaDto dto);
    Task<(bool Success, string Reply)> ObterRecentePorEstufaAsync(long idEstufa);
    Task<(bool Success, string Reply)> LimparAntigosAsync(int dias);
}

public class TelemetriaService : ITelemetriaService
{
    private readonly ITelemetriaRepository _telemetriaRepository;
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly IAlertaRepository _alertaRepository;

    public TelemetriaService(
        ITelemetriaRepository telemetriaRepository,
        IDispositivoRepository dispositivoRepository,
        IAlertaRepository alertaRepository)
    {
        _telemetriaRepository = telemetriaRepository;
        _dispositivoRepository = dispositivoRepository;
        _alertaRepository = alertaRepository;
    }

    public async Task<(bool Success, string Reply)> RegistrarAsync(TelemetriaDto dto)
    {
        // 1) Sanitizacao do identificador (Anti-XSS / payload corrompido)
        if (!InputSanitizer.UuidValido(dto.CodigoUuid))
            return (false, "CodigoUuid invalido: use apenas letras, numeros, hifen e underscore.");

        // 2) Validacao numerica: campos devem existir e ser numeros finitos
        if (!InputSanitizer.NumeroValido(dto.Temperatura) ||
            !InputSanitizer.NumeroValido(dto.Umidade) ||
            !InputSanitizer.NumeroValido(dto.Agua) ||
            !InputSanitizer.NumeroValido(dto.Luminosidade))
        {
            return (false, "Telemetria invalida: todos os valores devem ser numericos.");
        }

        double temp = dto.Temperatura!.Value;
        double umid = dto.Umidade!.Value;
        double agua = dto.Agua!.Value;
        double lumin = dto.Luminosidade!.Value;

        // 3) Limites fisicos reais (rejeita leituras impossiveis)
        var erroFisico = ValidarLimitesFisicos(temp, umid, agua, lumin);
        if (erroFisico is not null)
            return (false, erroFisico);

        // 4) Resolve o dispositivo a partir do UUID (consulta parametrizada)
        var (achou, dispReply) = await _dispositivoRepository.ObterPorUuidAsync(dto.CodigoUuid);
        if (!achou)
            return (false, dispReply);

        var dispositivo = Json.Deserialize<Dispositivo>(dispReply);
        if (dispositivo is null)
            return (false, "Falha ao resolver dispositivo.");

        // 5) Persiste a telemetria
        var (ok, reply) = await _telemetriaRepository.InserirAsync(dispositivo.IdDispositivo, temp, umid, agua, lumin);
        if (!ok)
            return (false, reply);

        // 6) Avalia regras e gera alertas automaticos
        var alertas = AvaliarAlertas(temp, umid, agua, lumin);
        var gerados = new List<object>();
        foreach (var (tipo, mensagem, gravidade) in alertas)
        {
            var (okAlerta, idAlerta) = await _alertaRepository.InserirAsync(dispositivo.IdDispositivo, tipo, mensagem, gravidade);
            if (okAlerta)
                gerados.Add(new { idAlerta = long.Parse(idAlerta), tipo, mensagem, gravidade });
        }

        var resumo = new
        {
            idTelemetria = long.Parse(reply),
            idDispositivo = dispositivo.IdDispositivo,
            codigoUuid = dispositivo.CodigoUuid,
            alertasGerados = gerados
        };

        return (true, Json.Serialize(resumo));
    }

    public Task<(bool Success, string Reply)> ObterRecentePorEstufaAsync(long idEstufa)
    {
        if (idEstufa <= 0)
            return Task.FromResult((false, "Id de estufa invalido."));
        return _telemetriaRepository.ObterRecentePorEstufaAsync(idEstufa);
    }

    public Task<(bool Success, string Reply)> LimparAntigosAsync(int dias)
    {
        if (dias < 0)
            return Task.FromResult((false, "Quantidade de dias invalida."));
        return _telemetriaRepository.LimparAntigosAsync(dias);
    }

    private static string? ValidarLimitesFisicos(double temp, double umid, double agua, double lumin)
    {
        if (temp < -50 || temp > 80) return "Temperatura fora dos limites fisicos (-50 a 80 C).";
        if (umid < 0 || umid > 100) return "Umidade fora dos limites fisicos (0 a 100 %).";
        if (agua < 0 || agua > 100) return "Nivel de agua fora dos limites fisicos (0 a 100 %).";
        if (lumin < 0 || lumin > 200000) return "Luminosidade fora dos limites fisicos (0 a 200000 lux).";
        return null;
    }

    // Regras de negocio: gera (Tipo, Mensagem, Gravidade) a partir da leitura
    private static List<(string Tipo, string Mensagem, string Gravidade)> AvaliarAlertas(
        double temp, double umid, double agua, double lumin)
    {
        var alertas = new List<(string, string, string)>();

        if (temp > 45)
            alertas.Add(("TEMPERATURA", $"Superaquecimento detectado: {temp} C.", "Critico"));
        else if (temp < 5)
            alertas.Add(("TEMPERATURA", $"Risco de congelamento: {temp} C.", "Critico"));
        else if (temp >= 38)
            alertas.Add(("TEMPERATURA", $"Temperatura elevada: {temp} C.", "Alerta"));

        if (umid < 20)
            alertas.Add(("UMIDADE", $"Umidade muito baixa (ressecamento): {umid} %.", "Critico"));
        else if (umid > 95)
            alertas.Add(("UMIDADE", $"Umidade muito alta (risco de fungos): {umid} %.", "Alerta"));

        if (agua < 15)
            alertas.Add(("AGUA", $"Reservatorio de agua critico: {agua} %.", "Critico"));
        else if (agua < 30)
            alertas.Add(("AGUA", $"Nivel de agua em atencao: {agua} %.", "Alerta"));

        if (lumin > 120000)
            alertas.Add(("LUMINOSIDADE", $"Luminosidade excessiva: {lumin} lux.", "Alerta"));
        else if (lumin < 1000)
            alertas.Add(("LUMINOSIDADE", $"Luminosidade baixa: {lumin} lux.", "Alerta"));

        return alertas;
    }
}
