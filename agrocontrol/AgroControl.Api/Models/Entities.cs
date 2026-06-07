namespace AgroControl.Api.Models;

// Entidades relacionais (espelham as tabelas T_AGC_*)

public class Usuario
{
    public long IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string DataCriacao { get; set; } = string.Empty;
}

public class Estufa
{
    public long IdEstufa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string TipoCultivo { get; set; } = string.Empty;
}

public class Dispositivo
{
    public long IdDispositivo { get; set; }
    public string CodigoUuid { get; set; } = string.Empty;
    public long IdEstufa { get; set; }
}

public class TelemetriaLog
{
    public long IdTelemetria { get; set; }
    public long IdDispositivo { get; set; }
    public double Temperatura { get; set; }
    public double Umidade { get; set; }
    public double Agua { get; set; }
    public double Luminosidade { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}

public class AlertaEvento
{
    public long IdAlerta { get; set; }
    public long IdDispositivo { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Gravidade { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string StResolvido { get; set; } = "N";
}
