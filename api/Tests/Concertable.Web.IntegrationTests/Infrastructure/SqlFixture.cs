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
            TablesToIgnore = ["__EFMigrationsHistory", "Users", "Genres", "ArtistGenres", "Artists", "Venues", "ConcertOpportunities", "Contracts", "OpportunityGenres"],
            DbAdapter = DbAdapter.SqlServer
        });
    }

    public async Task ResetAsync() 
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
