using System.ComponentModel.DataAnnotations;

namespace AgroControl.Api.Models.Dtos;

// Objetos de transporte (entrada da API)

public class CriarUsuarioDto
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Senha { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
}

public class TelemetriaDto
{
    // Identificacao do dispositivo (ex: 'ESTUFA-MARTE-01')
    [Required] public string CodigoUuid { get; set; } = string.Empty;

    [Required] public double? Temperatura { get; set; }
    [Required] public double? Umidade { get; set; }
    [Required] public double? Agua { get; set; }
    [Required] public double? Luminosidade { get; set; }
}
