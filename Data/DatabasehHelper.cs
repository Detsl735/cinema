using Npgsql;
using System.Data;

public class DatabaseHelper
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
