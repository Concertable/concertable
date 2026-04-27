using Concertable.Application.Interfaces.Geometry;
using Concertable.Shared.Infrastructure.Services.Geometry;
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
            var bands = new (string Name, string Banner, int[] GenreIds)[]
            {
                ("The Rockers", "rockers.jpg", [1, 2, 3]),
                ("Indie Vibes", "indievibes.jpg", [1, 5, 4]),
                ("Electronic Pulse", "electronicpulse.jpg", [5, 3]),
                ("Hip-Hop Flow", "hiphopflow.jpg", [4]),
                ("Jazz Masters", "jazzmaster.jpg", [6, 3]),
                ("Always Punks", "alwayspunks.jpg", [1, 6]),
                ("The Hollow Frequencies", "hollowfrequencies.jpg", [2]),
                ("Neon Foxes", "neonfoxes.jpg", [4, 2]),
                ("Velvet Static", "velvetstatic.jpg", [5, 3]),
                ("Echo Bloom", "echobloom.jpg", [1, 7]),
                ("The Wild Chords", "wildchords.jpg", [6, 1]),
                ("Glitch & Glow", "glitchandglow.jpg", [2]),
                ("Sonic Mirage", "sonicmirage.jpg", [6, 5]),
                ("Neon Echoes", "neonechoes.jpg", [4]),
                ("Dreamwave Collective", "dreamwavecollective.jpg", [7]),
                ("Synth Pulse", "synthpulse.jpg", [1]),
                ("The Brass Poets", "brasspoets.jpg", [3]),
                ("Groove Alchemy", "groovealchemy.jpg", [6]),
                ("Velvet Rhymes", "velvetrhymes.jpg", [4]),
                ("The Lo-Fi Syndicate", "lofisyndicate.jpg", [7]),
                ("Beats & Blue Notes", "beatsbluenotes.jpg", [8]),
                ("Bass Pilots", "basspilots.jpg", [1]),
                ("The Digital Prophets", "digitalprophets.jpg", [5]),
                ("Neon Bass Theory", "neonbasstheory.jpg", [6]),
                ("Wavelength 303", "wavelength303.jpg", [2]),
                ("Gravity Loops", "gravityloops.jpg", [1]),
                ("The Golden Reverie", "goldenreverie.jpg", [8]),
                ("Fable Sound", "fablesound.jpg", [5]),
                ("Moonlight Static", "moonlightstatic.jpg", [7]),
                ("The Chromatics", "thechromatics.jpg", [3]),
                ("Echo Reverberation", "echoreverberation.jpg", [6]),
                ("Midnight Reverie", "midnightreverie.jpg", [1]),
                ("Static Wolves", "staticwolves.jpg", [4]),
                ("Echo Collapse", "echocollapse.jpg", [2]),
                ("Violet Sundown", "violetsundown.jpg", [8])
            };

            var artists = bands.Select((b, i) =>
            {
                var loc = locationFaker.Next();
                return ArtistFaker.GetFaker(
                    artistManagerIds[i],
                    b.Name,
                    b.Banner,
                    "avatar.jpg",
                    geometryProvider.CreatePoint(loc.Latitude, loc.Longitude),
                    new Address(loc.County, loc.Town),
                    $"{b.Name.ToLowerInvariant().Replace(" ", "")}@test.com",
                    b.GenreIds).Generate();
            }).ToArray();

            context.Artists.AddRange(artists);
            await context.SaveChangesAsync(ct);
        });
    }
}
