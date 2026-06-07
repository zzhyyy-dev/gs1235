using System.Data;
using Microsoft.Data.Sqlite;

namespace AgroControl.Api.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        // Garante a aplicacao das chaves estrangeiras a cada conexao
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
