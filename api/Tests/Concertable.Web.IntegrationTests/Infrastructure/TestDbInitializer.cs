using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly IGeometryProvider geometryProvider;
    private readonly SeedData seed;
    private readonly ILocationFaker locationFaker;
    private readonly TimeProvider timeProvider;
    private readonly IEnumerable<ITestSeeder> seeders;

    public TestDbInitializer(
        ApplicationDbContext context,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        SeedData seed,
        ILocationFaker locationFaker,
        TimeProvider timeProvider,
        IEnumerable<ITestSeeder> seeders)
    {
        this.context = context;
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

        await context.Database.MigrateAsync();

        // Genres are reference data used by module seeders (ArtistTestSeeder needs seed.Rock),
        // so they must be seeded before the SeedAsync loop runs.
        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            seed.Rock = GenreFactory.Create("Rock");
            seed.Jazz = GenreFactory.Create("Jazz");
            seed.HipHop = GenreFactory.Create("Hip-Hop");
            seed.Electronic = GenreFactory.Create("Electronic");

            context.Genres.AddRange(seed.Rock, seed.Jazz, seed.HipHop, seed.Electronic);
            await context.SaveChangesAsync();
        });

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}
