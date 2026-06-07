using AgroControl.Api.Data;
using AgroControl.Api.Repositories;
using AgroControl.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=agrocontrol.db";

// ---- Camada de dados ----
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();

// ---- Repositories ----
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddScoped<ITelemetriaRepository, TelemetriaRepository>();
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();

// ---- Services ----
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITelemetriaService, TelemetriaService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgroControl API",
        Version = "v1",
        Description = "Backend de monitoramento de estufas (telemetria + alertas)."
    });
});

var app = builder.Build();

// Cria o schema e o seed a partir do script sql/schema.sql
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroControl API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.Run();
