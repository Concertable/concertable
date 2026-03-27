using System.Data.Common;
using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;
using Xunit;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class SqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _dbConnection = new SqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();
    }

    public async Task InitializeRespawnerAsync()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.SqlServer,
            WithReseed = true // resets IDENTITY counters to 0 (next insert = 1) so hardcoded IDs in TestDbInitializer remain valid after each reset
        });
    }

    public async Task ResetAsync() => await _respawner.ResetAsync(_dbConnection);

    public async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
