using System.Text.Json;

namespace AgroControl.Api.Repositories;

// Helper interno: serializa payloads de leitura para o campo "Reply" (string)
// mantendo o contrato (bool Success, string Reply) em todas as camadas.
internal static class Json
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, ReadOptions);
}
