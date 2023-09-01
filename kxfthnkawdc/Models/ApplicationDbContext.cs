using Npgsql;

namespace kxfthnkawdc.Models;

public sealed class ApplicationDbContext
{
    private const string ConnectionString = "Host=178.162.94.95;Database=kxfthnkawdc;Username=kxfthnkawdc;Password=kxfthnkawdc";
    public NpgsqlDataSource DataSource;
    public readonly NpgsqlConnection Connection;

    public ApplicationDbContext()
    {
        DataSource = new NpgsqlDataSourceBuilder(ConnectionString).Build();
        Connection = DataSource.OpenConnection();
    }
}
