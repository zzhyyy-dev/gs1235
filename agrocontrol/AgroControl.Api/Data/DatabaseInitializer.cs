using System.Data;
using Dapper;

namespace AgroControl.Api.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _factory;

    public DatabaseInitializer(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Initialize()
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "sql", "schema.sql");
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script de schema nao encontrado: {scriptPath}");

        var script = File.ReadAllText(scriptPath);
        using IDbConnection connection = _factory.CreateConnection();
        connection.Execute(script);
    }
}
