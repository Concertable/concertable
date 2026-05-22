using System.Data.Common;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Data.SqlClient;
using Respawn;

namespace Concertable.E2ETests;

public class SqlFixture
{
    private DbConnection connection = null!;
    private DbConnection paymentConnection = null!;
    private Respawner respawner = null!;

    public DbConnection Connection => connection;
    public DbConnection PaymentConnection => paymentConnection;

    public async Task InitializeAsync(DistributedApplication app)
    {
        connection = await OpenConnectionAsync(app, "B2BDb");
        paymentConnection = await OpenConnectionAsync(app, "PaymentDb");

        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.SqlServer,
            WithReseed = true
        });
    }

    public async Task ResetAsync() => await respawner.ResetAsync(connection);

    public async Task DisposeAsync()
    {
        await connection.DisposeAsync();
        await paymentConnection.DisposeAsync();
    }

    private static async Task<DbConnection> OpenConnectionAsync(DistributedApplication app, string database)
    {
        var connectionString = await app.GetConnectionStringAsync(database);
        var builder = new SqlConnectionStringBuilder(connectionString) { MultipleActiveResultSets = true };
        var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
