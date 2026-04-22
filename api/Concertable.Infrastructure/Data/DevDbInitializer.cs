using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Seeding.Factories;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Data;

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

        // Genres are reference data used by module seeders (ArtistDevSeeder assigns genre IDs),
        // so they must be seeded before the SeedAsync loop runs.
        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            var genres = new GenreEntity[]
            {
                GenreFactory.Create("Rock"),
                GenreFactory.Create("Pop"),
                GenreFactory.Create("Jazz"),
                GenreFactory.Create("Hip-Hop"),
                GenreFactory.Create("Electronic"),
                GenreFactory.Create("Indie"),
                GenreFactory.Create("DnB"),
                GenreFactory.Create("House")
            };
            context.Genres.AddRange(genres);
            await context.SaveChangesAsync();
        });

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var customerIds = seedData.CustomerIds;
        var artistManagerIds = seedData.ArtistManagerIds;
        var venueManagerIds = seedData.VenueManagerIds;

        await context.Preferences.SeedIfEmptyAsync(async () =>
        {
            var preferences = new PreferenceEntity[]
            {
                PreferenceFactory.Create(customerIds[0], 10),
                PreferenceFactory.Create(customerIds[1], 25),
                PreferenceFactory.Create(customerIds[2], 50),
            };
            context.Preferences.AddRange(preferences);
            await context.SaveChangesAsync();
        });

        await context.GenrePreferences.SeedIfEmptyAsync(async () =>
        {
            context.GenrePreferences.Add(new GenrePreferenceEntity { PreferenceId = 1, GenreId = 1 });
            await context.SaveChangesAsync();
        });

        await context.Transactions.SeedIfEmptyAsync(async () =>
        {
            var settlementTransactions = new[]
            {
                TransactionFactory.Settlement(venueManagerIds[0], artistManagerIds[0], Guid.NewGuid().ToString(), 15000, TransactionStatus.Complete, 1),
                TransactionFactory.Settlement(venueManagerIds[1], artistManagerIds[1], Guid.NewGuid().ToString(), 20000, TransactionStatus.Complete, 2),
                TransactionFactory.Settlement(venueManagerIds[2], artistManagerIds[2], Guid.NewGuid().ToString(), 18000, TransactionStatus.Complete, 3),
                TransactionFactory.Settlement(venueManagerIds[3], artistManagerIds[3], Guid.NewGuid().ToString(), 17500, TransactionStatus.Complete, 4),
                TransactionFactory.Settlement(venueManagerIds[4], artistManagerIds[4], Guid.NewGuid().ToString(), 16000, TransactionStatus.Complete, 5),
            };
            settlementTransactions[0].CreatedAt = now.AddDays(-58);
            settlementTransactions[1].CreatedAt = now.AddDays(-55);
            settlementTransactions[2].CreatedAt = now.AddDays(-52);
            settlementTransactions[3].CreatedAt = now.AddDays(-49);
            settlementTransactions[4].CreatedAt = now.AddDays(-46);

            var ticketTransactions = new[]
            {
                TransactionFactory.Ticket(customerIds[0], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[1], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[2], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[3], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[4], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[5], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(artistManagerIds[0], venueManagerIds[0], Guid.NewGuid().ToString(), 1500, TransactionStatus.Complete, 1),
                TransactionFactory.Ticket(customerIds[1], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(customerIds[2], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(customerIds[3], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(customerIds[4], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(customerIds[5], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(artistManagerIds[0], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(artistManagerIds[1], venueManagerIds[1], Guid.NewGuid().ToString(), 1200, TransactionStatus.Complete, 2),
                TransactionFactory.Ticket(customerIds[2], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(customerIds[3], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(customerIds[4], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(customerIds[5], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(artistManagerIds[0], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(artistManagerIds[1], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
                TransactionFactory.Ticket(artistManagerIds[2], venueManagerIds[2], Guid.NewGuid().ToString(), 1800, TransactionStatus.Complete, 3),
            };

            context.SettlementTransactions.AddRange(settlementTransactions);
            context.TicketTransactions.AddRange(ticketTransactions);
            await context.SaveChangesAsync();
        });
    }
}
