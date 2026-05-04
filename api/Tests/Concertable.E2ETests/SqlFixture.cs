using System.Data.Common;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Data.SqlClient;
using Respawn;

namespace Concertable.E2ETests;

public class SqlFixture
{
    private DbConnection connection = null!;
    private Respawner respawner = null!;

    public DbConnection Connection => connection;

    public async Task InitializeAsync(DistributedApplication app)
    {
        var connectionString = await app.GetConnectionStringAsync("DefaultConnection");
        connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.SqlServer,
            WithReseed = true
        });
    }

    public async Task ResetAsync() => await respawner.ResetAsync(connection);

    public async Task DisposeAsync() => await connection.DisposeAsync();
}
