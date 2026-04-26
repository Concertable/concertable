using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Data;
using Concertable.Seeding;
using Concertable.Seeding.Fakers;
using Concertable.Shared.Infrastructure.Services.Geometry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web;

public class DevDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly SeedData seedData;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationFaker locationFaker;
    private readonly IEnumerable<IDevSeeder> seeders;

    public DevDbInitializer(
        ApplicationDbContext context,
        SeedData seedData,
        TimeProvider timeProvider,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        ILocationFaker locationFaker,
        IEnumerable<IDevSeeder> seeders)
    {
        this.context = context;
        this.seedData = seedData;
        this.timeProvider = timeProvider;
        this.geometryProvider = geometryProvider;
        this.locationFaker = locationFaker;
        this.seeders = seeders;
    }

    public async Task InitializeAsync()
    {
        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.MigrateAsync();

        await context.Database.MigrateAsync();

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}
