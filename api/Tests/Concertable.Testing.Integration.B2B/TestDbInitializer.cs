using Concertable.DataAccess;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.Seeding;
using Concertable.Seeding.Fakers;
using Concertable.Shared.Infrastructure.Services.Geometry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Testing.Integration;

public class TestDbInitializer : IDbInitializer
{
    private readonly IGeometryProvider geometryProvider;
    private readonly SeedData seed;
    private readonly ILocationFaker locationFaker;
    private readonly TimeProvider timeProvider;
    private readonly IEnumerable<ITestSeeder> seeders;
    private readonly InboxDbContext inboxDbContext;
    private readonly OutboxDbContext outboxDbContext;

    public TestDbInitializer(
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        SeedData seed,
        ILocationFaker locationFaker,
        TimeProvider timeProvider,
        IEnumerable<ITestSeeder> seeders,
        InboxDbContext inboxDbContext,
        OutboxDbContext outboxDbContext)
    {
        this.geometryProvider = geometryProvider;
        this.seed = seed;
        this.locationFaker = locationFaker;
        this.timeProvider = timeProvider;
        this.seeders = seeders;
        this.inboxDbContext = inboxDbContext;
        this.outboxDbContext = outboxDbContext;
    }

    public async Task InitializeAsync()
    {
        await inboxDbContext.Database.MigrateAsync();
        await outboxDbContext.Database.MigrateAsync();

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.MigrateAsync();

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}
