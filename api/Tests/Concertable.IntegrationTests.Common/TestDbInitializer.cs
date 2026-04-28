using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Fakers;
using Concertable.Shared.Infrastructure.Services.Geometry;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.IntegrationTests.Common;

public class TestDbInitializer : IDbInitializer
{
    private readonly IGeometryProvider geometryProvider;
    private readonly SeedData seed;
    private readonly ILocationFaker locationFaker;
    private readonly TimeProvider timeProvider;
    private readonly IEnumerable<ITestSeeder> seeders;

    public TestDbInitializer(
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        SeedData seed,
        ILocationFaker locationFaker,
        TimeProvider timeProvider,
        IEnumerable<ITestSeeder> seeders)
    {
        this.geometryProvider = geometryProvider;
        this.seed = seed;
        this.locationFaker = locationFaker;
        this.timeProvider = timeProvider;
        this.seeders = seeders;
    }

    public async Task InitializeAsync()
    {
        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.MigrateAsync();

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}
