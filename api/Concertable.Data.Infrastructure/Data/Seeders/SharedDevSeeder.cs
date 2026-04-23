using Concertable.Application.Interfaces;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Data.Seeders;

internal class SharedDevSeeder : IDevSeeder
{
    public int Order => -1;

    private readonly SharedDbContext context;

    public SharedDevSeeder(SharedDbContext context)
    {
        this.context = context;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
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
            await context.SaveChangesAsync(ct);
        });
    }
}
