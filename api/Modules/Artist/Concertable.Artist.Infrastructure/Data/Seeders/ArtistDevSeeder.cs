using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Fakers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Artist.Infrastructure.Data.Seeders;

internal class ArtistDevSeeder : IDevSeeder
{
    public int Order => 1;

    private readonly ArtistDbContext context;
    private readonly SeedData seed;
    private readonly IGeometryProvider geometryProvider;
    private readonly ILocationFaker locationFaker;

    public ArtistDevSeeder(
        ArtistDbContext context,
        SeedData seed,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        ILocationFaker locationFaker)
    {
        this.context = context;
        this.seed = seed;
        this.geometryProvider = geometryProvider;
        this.locationFaker = locationFaker;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var artistManagerIds = seed.ArtistManagerIds;

        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            var artists = new ArtistEntity[]
            {
                ArtistFaker.GetFaker(artistManagerIds[0], "The Rockers", "rockers.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[1], "Indie Vibes", "indievibes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[2], "Electronic Pulse", "electronicpulse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[3], "Hip-Hop Flow", "hiphopflow.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[4], "Jazz Masters", "jazzmaster.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[5], "Always Punks", "alwayspunks.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[6], "The Hollow Frequencies", "hollowfrequencies.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[7], "Neon Foxes", "neonfoxes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[8], "Velvet Static", "velvetstatic.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[9], "Echo Bloom", "echobloom.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[10], "The Wild Chords", "wildchords.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[11], "Glitch & Glow", "glitchandglow.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[12], "Sonic Mirage", "sonicmirage.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[13], "Neon Echoes", "neonechoes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[14], "Dreamwave Collective", "dreamwavecollective.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[15], "Synth Pulse", "synthpulse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[16], "The Brass Poets", "brasspoets.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[17], "Groove Alchemy", "groovealchemy.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[18], "Velvet Rhymes", "velvetrhymes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[19], "The Lo-Fi Syndicate", "lofisyndicate.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[20], "Beats & Blue Notes", "beatsbluenotes.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[21], "Bass Pilots", "basspilots.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[22], "The Digital Prophets", "digitalprophets.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[23], "Neon Bass Theory", "neonbasstheory.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[24], "Wavelength 303", "wavelength303.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[25], "Gravity Loops", "gravityloops.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[26], "The Golden Reverie", "goldenreverie.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[27], "Fable Sound", "fablesound.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[28], "Moonlight Static", "moonlightstatic.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[29], "The Chromatics", "thechromatics.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[30], "Echo Reverberation", "echoreverberation.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[31], "Midnight Reverie", "midnightreverie.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[32], "Static Wolves", "staticwolves.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[33], "Echo Collapse", "echocollapse.jpg").Generate(),
                ArtistFaker.GetFaker(artistManagerIds[34], "Violet Sundown", "violetsundown.jpg").Generate()
            };
            foreach (var artist in artists)
            {
                var loc = locationFaker.Next();
                artist.Location = geometryProvider.CreatePoint(loc.Latitude, loc.Longitude);
                artist.Address = new Address(loc.County, loc.Town);
                artist.Email = string.Empty;
            }

            context.Artists.AddRange(artists);
            await context.SaveChangesAsync(ct);
        });

        await context.ArtistGenres.SeedIfEmptyAsync(async () =>
        {
            var artistGenres = new ArtistGenreEntity[]
            {
                new ArtistGenreEntity { ArtistId = 1, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 1, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 1, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 2, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 3, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 3, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 4, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 5, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 5, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 6, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 6, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 7, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 8, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 8, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 9, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 9, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 10, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 10, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 11, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 11, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 12, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 13, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 13, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 14, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 15, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 16, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 17, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 18, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 19, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 20, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 21, GenreId = 8 },
                new ArtistGenreEntity { ArtistId = 22, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 23, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 24, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 25, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 26, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 27, GenreId = 8 },
                new ArtistGenreEntity { ArtistId = 28, GenreId = 5 },
                new ArtistGenreEntity { ArtistId = 29, GenreId = 7 },
                new ArtistGenreEntity { ArtistId = 30, GenreId = 3 },
                new ArtistGenreEntity { ArtistId = 31, GenreId = 6 },
                new ArtistGenreEntity { ArtistId = 32, GenreId = 1 },
                new ArtistGenreEntity { ArtistId = 33, GenreId = 4 },
                new ArtistGenreEntity { ArtistId = 34, GenreId = 2 },
                new ArtistGenreEntity { ArtistId = 35, GenreId = 8 },
            };
            context.ArtistGenres.AddRange(artistGenres);
            await context.SaveChangesAsync(ct);
        });
    }
}
