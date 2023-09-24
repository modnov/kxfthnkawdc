using Npgsql;

namespace kxfthnkawdc.Models;

public sealed class ApplicationDbContext
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;
    public NpgsqlDataSource DataSource;

    public ApplicationDbContext()
    {
        DataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
    }
}
