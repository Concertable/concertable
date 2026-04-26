using Concertable.Application.Interfaces;
using Concertable.Messaging.Contracts;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Messaging.Infrastructure.Data.Seeders;

internal class MessagingTestSeeder : ITestSeeder
{
    public int Order => 6;

    private readonly MessagingDbContext context;
    private readonly SeedData seedData;
    private readonly TimeProvider timeProvider;

    public MessagingTestSeeder(MessagingDbContext context, SeedData seedData, TimeProvider timeProvider)
    {
        this.context = context;
        this.seedData = seedData;
        this.timeProvider = timeProvider;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Messages.SeedIfEmptyAsync(async () =>
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;

            context.Messages.AddRange(
                MessageEntity.Create(
                    seedData.ArtistManager.Id,
                    seedData.VenueManager1.Id,
                    "Test inbox message — artist to venue.",
                    now.AddDays(-1),
                    MessageAction.ApplicationReceived),
                MessageEntity.Create(
                    seedData.VenueManager1.Id,
                    seedData.ArtistManager.Id,
                    "Test inbox message — venue to artist.",
                    now,
                    MessageAction.ApplicationAccepted));

            await context.SaveChangesAsync(ct);
        });
    }
}
