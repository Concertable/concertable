using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Fakers;
using Concertable.Venue.Contracts.Events;
using Concertable.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Venue.Infrastructure.Data.Seeders;

internal class VenueTestSeeder : ITestSeeder
{
    public int Order => 2;

    private readonly VenueDbContext context;
    private readonly SeedData seed;
    private readonly IIntegrationEventBus eventBus;

    public VenueTestSeeder(VenueDbContext context, SeedData seed, IIntegrationEventBus eventBus)
    {
        this.context = context;
        this.seed = seed;
        this.eventBus = eventBus;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            seed.Venue = VenueFaker.GetFaker(seed.VenueManager1.Id, "Test Venue", "venue.jpg").Generate();
            seed.Venue.Location = seed.VenueManager1.Location;
            seed.Venue.Address = seed.VenueManager1.Address;
            seed.Venue.Avatar = seed.VenueManager1.Avatar;
            seed.Venue.Email = seed.VenueManager1.Email;

            context.Venues.Add(seed.Venue);
            await context.SaveChangesAsync(ct);

            await eventBus.PublishAsync(new VenueChangedEvent(
                seed.Venue.Id,
                seed.Venue.UserId,
                seed.Venue.Name,
                seed.Venue.About,
                seed.Venue.Address?.County,
                seed.Venue.Address?.Town,
                seed.Venue.Location?.Y,
                seed.Venue.Location?.X), ct);
        });
    }
}
