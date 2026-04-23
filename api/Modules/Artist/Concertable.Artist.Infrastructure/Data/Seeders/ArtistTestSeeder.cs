using Concertable.Artist.Contracts.Events;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Data.Seeders;

internal class ArtistTestSeeder : ITestSeeder
{
    public int Order => 1;

    private readonly ArtistDbContext context;
    private readonly SeedData seed;
    private readonly IIntegrationEventBus eventBus;

    public ArtistTestSeeder(ArtistDbContext context, SeedData seed, IIntegrationEventBus eventBus)
    {
        this.context = context;
        this.seed = seed;
        this.eventBus = eventBus;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            seed.Artist = ArtistFaker.GetFaker(seed.ArtistManager.Id, "Test Artist", "artist.jpg").Generate();
            seed.Artist.SyncGenres([seed.Rock.Id]);
            seed.Artist.Location = seed.ArtistManager.Location;
            seed.Artist.Address = seed.ArtistManager.Address;
            seed.Artist.Avatar = seed.ArtistManager.Avatar;
            seed.Artist.Email = seed.ArtistManager.Email;

            context.Artists.Add(seed.Artist);
            await context.SaveChangesAsync(ct);

            await eventBus.PublishAsync(new ArtistChangedEvent(
                seed.Artist.Id,
                seed.Artist.UserId,
                seed.Artist.Name,
                seed.Artist.Avatar,
                seed.Artist.BannerUrl,
                seed.Artist.Address?.County,
                seed.Artist.Address?.Town,
                seed.Artist.Email,
                [seed.Rock.Id]), ct);
        });
    }
}
