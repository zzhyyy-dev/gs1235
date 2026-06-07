using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AgroControl.Api.Models.Dtos;
using AgroControl.Api.Repositories;
using AgroControl.Api.Services;

namespace AgroControl.Tests;

// Manual Mocks to avoid external mocking package dependencies
public class MockDispositivoRepository : IDispositivoRepository
{
    public bool Exist { get; set; } = true;
    public string ResultJson { get; set; } = "{\"IdDispositivo\":1,\"CodigoUuid\":\"ESTUFA-MARTE-01\",\"IdEstufa\":1}";

    public Task<(bool Success, string Reply)> ObterPorUuidAsync(string uuid)
    {
        if (Exist)
            return Task.FromResult((true, ResultJson));
        return Task.FromResult((false, "Dispositivo nao encontrado."));
    }
}

public class MockTelemetriaRepository : ITelemetriaRepository
{
    public Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, double temperatura, double umidade, double agua, double luminosidade)
    {
        return Task.FromResult((true, "42")); // Mock inserted ID
    }

    public Task<(bool Success, string Reply)> ObterRecentePorEstufaAsync(long idEstufa) => throw new System.NotImplementedException();
    public Task<(bool Success, string Reply)> LimparAntigosAsync(int dias) => throw new System.NotImplementedException();
}

public class MockAlertaRepository : IAlertaRepository
{
    public List<(long idDispositivo, string tipo, string mensagem, string gravidade)> InsertedAlerts { get; } = new();

    public Task<(bool Success, string Reply)> InserirAsync(long idDispositivo, string tipo, string mensagem, string gravidade)
    {
        InsertedAlerts.Add((idDispositivo, tipo, mensagem, gravidade));
        return Task.FromResult((true, "1")); // Mock inserted ID
    }

    public Task<(bool Success, string Reply)> ListarCriticosAtivosAsync() => throw new System.NotImplementedException();
    public Task<(bool Success, string Reply)> ResolverAsync(long idAlerta) => throw new System.NotImplementedException();
}

public class TelemetriaServiceTests
{
    private readonly MockDispositivoRepository _mockDispRepo;
    private readonly MockTelemetriaRepository _mockTelemRepo;
    private readonly MockAlertaRepository _mockAlertaRepo;
    private readonly TelemetriaService _service;

    public TelemetriaServiceTests()
    {
        _mockDispRepo = new MockDispositivoRepository();
        _mockTelemRepo = new MockTelemetriaRepository();
        _mockAlertaRepo = new MockAlertaRepository();
        _service = new TelemetriaService(_mockTelemRepo, _mockDispRepo, _mockAlertaRepo);
    }

    [Fact]
    public async Task RegistrarAsync_ShouldFail_WhenUuidIsInvalid()
    {
        // Arrange
        var dto = new TelemetriaDto
        {
            CodigoUuid = "INVALID UUID!",
            Temperatura = 25.0,
            Umidade = 50.0,
            Agua = 50.0,
            Luminosidade = 5000.0
        };

        // Act
        var (success, reply) = await _service.RegistrarAsync(dto);

        // Assert
        Assert.False(success);
        Assert.Contains("CodigoUuid invalido", reply);
    }

    [Theory]
    [InlineData(-51.0, 50.0, 50.0, 5000.0, "Temperatura fora dos limites")]
    [InlineData(81.0, 50.0, 50.0, 5000.0, "Temperatura fora dos limites")]
    [InlineData(25.0, -1.0, 50.0, 5000.0, "Umidade fora dos limites")]
    [InlineData(25.0, 101.0, 50.0, 5000.0, "Umidade fora dos limites")]
    [InlineData(25.0, 50.0, -1.0, 5000.0, "Nivel de agua fora dos limites")]
    [InlineData(25.0, 50.0, 101.0, 5000.0, "Nivel de agua fora dos limites")]
    [InlineData(25.0, 50.0, 50.0, -1.0, "Luminosidade fora dos limites")]
    [InlineData(25.0, 50.0, 50.0, 200001.0, "Luminosidade fora dos limites")]
    public async Task RegistrarAsync_ShouldFail_WhenPhysicalLimitsAreViolated(
        double temp, double umid, double agua, double lumin, string expectedErrorSub)
    {
        // Arrange
        var dto = new TelemetriaDto
        {
            CodigoUuid = "ESTUFA-MARTE-01",
            Temperatura = temp,
            Umidade = umid,
            Agua = agua,
            Luminosidade = lumin
        };

        // Act
        var (success, reply) = await _service.RegistrarAsync(dto);

        // Assert
        Assert.False(success);
        Assert.Contains(expectedErrorSub, reply);
    }

    [Fact]
    public async Task RegistrarAsync_ShouldSucceedAndGenerateNoAlerts_WhenDataIsNormal()
    {
        // Arrange
        var dto = new TelemetriaDto
        {
            CodigoUuid = "ESTUFA-MARTE-01",
            Temperatura = 24.5,
            Umidade = 60.0,
            Agua = 80.0,
            Luminosidade = 40000.0
        };

        // Act
        var (success, reply) = await _service.RegistrarAsync(dto);

        // Assert
        Assert.True(success);
        Assert.Contains("idTelemetria", reply);
        Assert.Empty(_mockAlertaRepo.InsertedAlerts);
    }

    [Fact]
    public async Task RegistrarAsync_ShouldGenerateCriticalAlerts_WhenDataIsExtreme()
    {
        // Arrange
        var dto = new TelemetriaDto
        {
            CodigoUuid = "ESTUFA-MARTE-01",
            Temperatura = 52.0,       // Superaquecimento (> 45) -> Critico
            Umidade = 12.0,           // Ressecamento (< 20) -> Critico
            Agua = 5.0,               // Nivel critico (< 15) -> Critico
            Luminosidade = 150000.0   // Luminosidade excessiva (> 120000) -> Alerta
        };

        // Act
        var (success, reply) = await _service.RegistrarAsync(dto);

        // Assert
        Assert.True(success);
        Assert.Contains("alertasGerados", reply);
        
        var alerts = _mockAlertaRepo.InsertedAlerts;
        Assert.Equal(4, alerts.Count);

        // Verify critical alerts
        Assert.Contains(alerts, a => a.tipo == "TEMPERATURA" && a.gravidade == "Critico" && a.mensagem.Contains("Superaquecimento"));
        Assert.Contains(alerts, a => a.tipo == "UMIDADE" && a.gravidade == "Critico" && a.mensagem.Contains("Umidade muito baixa"));
        Assert.Contains(alerts, a => a.tipo == "AGUA" && a.gravidade == "Critico" && a.mensagem.Contains("Reservatorio de agua critico"));
        Assert.Contains(alerts, a => a.tipo == "LUMINOSIDADE" && a.gravidade == "Alerta" && a.mensagem.Contains("Luminosidade excessiva"));
    }
}
