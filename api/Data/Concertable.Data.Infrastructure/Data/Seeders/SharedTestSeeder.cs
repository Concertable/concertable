using Concertable.Application.Interfaces;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Data.Seeders;

internal class SharedTestSeeder : ITestSeeder
{
    public int Order => -1;

    private readonly SharedDbContext context;
    private readonly SeedData seed;

    public SharedTestSeeder(SharedDbContext context, SeedData seed)
    {
        this.context = context;
        this.seed = seed;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            seed.Rock = GenreFactory.Create("Rock");
            seed.Jazz = GenreFactory.Create("Jazz");
            seed.HipHop = GenreFactory.Create("Hip-Hop");
            seed.Electronic = GenreFactory.Create("Electronic");

            context.Genres.AddRange(seed.Rock, seed.Jazz, seed.HipHop, seed.Electronic);
            await context.SaveChangesAsync(ct);
        });
    }
}
