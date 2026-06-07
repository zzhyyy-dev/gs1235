using AgroControl.Api.Repositories;

namespace AgroControl.Api.Services;

public interface IAlertaService
{
    Task<(bool Success, string Reply)> ListarCriticosAsync();
    Task<(bool Success, string Reply)> ResolverAsync(long idAlerta);
}

public class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _repository;

    public AlertaService(IAlertaRepository repository) => _repository = repository;

    public Task<(bool Success, string Reply)> ListarCriticosAsync() =>
        _repository.ListarCriticosAtivosAsync();

    public Task<(bool Success, string Reply)> ResolverAsync(long idAlerta)
    {
        if (idAlerta <= 0)
            return Task.FromResult((false, "Id de alerta invalido."));
        return _repository.ResolverAsync(idAlerta);
    }
}
