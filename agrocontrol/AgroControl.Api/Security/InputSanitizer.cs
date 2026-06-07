using System.Text.RegularExpressions;

namespace AgroControl.Api.Security;

// Sanitizacao de inputs (Anti-XSS / Anti-Payload corrompido)
public static class InputSanitizer
{
    // Detecta tags HTML/script ou caracteres tipicos de injecao
    private static readonly Regex ConteudoSuspeito =
        new(@"[<>]|script|javascript:|onerror|onload|--|;|/\*|\*/", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // UUID do dispositivo: apenas letras, numeros, hifen e underscore
    private static readonly Regex UuidPermitido =
        new(@"^[A-Za-z0-9_-]{1,60}$", RegexOptions.Compiled);

    public static bool ContemConteudoSuspeito(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return false;
        return ConteudoSuspeito.IsMatch(valor);
    }

    public static bool UuidValido(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return false;
        return UuidPermitido.IsMatch(valor);
    }

    // Numeros devem ser finitos (rejeita NaN / Infinity vindos de payload corrompido)
    public static bool NumeroValido(double? valor) =>
        valor.HasValue && !double.IsNaN(valor.Value) && !double.IsInfinity(valor.Value);
}
