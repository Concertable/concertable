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

    public ArtistTestSeeder(ArtistDbContext context, SeedData seed)
    {
        this.context = context;
        this.seed = seed;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            seed.Artist = ArtistFaker.GetFaker(seed.ArtistManager.Id, "Test Artist", "artist.jpg").Generate();
            seed.Artist.Location = seed.ArtistManager.Location;
            seed.Artist.Address = seed.ArtistManager.Address;
            seed.Artist.Avatar = seed.ArtistManager.Avatar;
            seed.Artist.Email = seed.ArtistManager.Email;
            seed.Artist.SyncGenres([seed.Rock.Id]);

            context.Artists.Add(seed.Artist);
            await context.SaveChangesAsync(ct);
        });
    }
}
