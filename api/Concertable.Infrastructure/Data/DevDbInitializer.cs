using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;
using Concertable.Seeding.Fakers;
using Concertable.Seeding.Factories;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
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

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            var opportunities = new OpportunityEntity[]
            {
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-60), now.AddDays(-60).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(-55), now.AddDays(-55).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(-50), now.AddDays(-50).AddHours(3)), FlatFeeContractEntity.Create(180, PaymentMethod.Cash)),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(-45), now.AddDays(-45).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(-40), now.AddDays(-40).AddHours(3)), FlatFeeContractEntity.Create(160, PaymentMethod.Cash)),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(-35), now.AddDays(-35).AddHours(3)), FlatFeeContractEntity.Create(220, PaymentMethod.Cash)),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(-30), now.AddDays(-30).AddHours(3)), FlatFeeContractEntity.Create(210, PaymentMethod.Cash)),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(-25), now.AddDays(-25).AddHours(3)), FlatFeeContractEntity.Create(230, PaymentMethod.Cash)),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(-20), now.AddDays(-20).AddHours(3)), FlatFeeContractEntity.Create(240, PaymentMethod.Cash)),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(-15), now.AddDays(-15).AddHours(3)), FlatFeeContractEntity.Create(250, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-10), now.AddDays(-10).AddHours(3)), FlatFeeContractEntity.Create(160, PaymentMethod.Cash)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(-5), now.AddDays(-5).AddHours(3)), FlatFeeContractEntity.Create(300, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now, now.AddHours(3)), FlatFeeContractEntity.Create(280, PaymentMethod.Cash)),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(5), now.AddDays(5).AddHours(3)), FlatFeeContractEntity.Create(270, PaymentMethod.Transfer)),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash)),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), DoorSplitContractEntity.Create(65, PaymentMethod.Cash)),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(20), now.AddDays(20).AddHours(3)), VersusContractEntity.Create(150, 70, PaymentMethod.Cash)),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(25), now.AddDays(25).AddHours(3)), VersusContractEntity.Create(200, 70, PaymentMethod.Transfer)),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), FlatFeeContractEntity.Create(245, PaymentMethod.Transfer)),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(35), now.AddDays(35).AddHours(3)), FlatFeeContractEntity.Create(240, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-40), now.AddDays(-40).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Transfer)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(45), now.AddDays(45).AddHours(3)), FlatFeeContractEntity.Create(230, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(50), now.AddDays(50).AddHours(3)), FlatFeeContractEntity.Create(225, PaymentMethod.Cash)),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(55), now.AddDays(55).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash)),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(60), now.AddDays(60).AddHours(3)), FlatFeeContractEntity.Create(215, PaymentMethod.Cash)),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(65), now.AddDays(65).AddHours(3)), FlatFeeContractEntity.Create(210, PaymentMethod.Cash)),
                OpportunityFactory.Create(7, new DateRange(now.AddDays(70), now.AddDays(70).AddHours(3)), FlatFeeContractEntity.Create(205, PaymentMethod.Cash)),
                OpportunityFactory.Create(8, new DateRange(now.AddDays(75), now.AddDays(75).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityFactory.Create(9, new DateRange(now.AddDays(80), now.AddDays(80).AddHours(3)), FlatFeeContractEntity.Create(195, PaymentMethod.Cash)),
                OpportunityFactory.Create(10, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(3)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-85), now.AddDays(-85).AddHours(3)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(85), now.AddDays(85).AddHours(5)), FlatFeeContractEntity.Create(190, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(2), now.AddDays(2).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(4), now.AddDays(4).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(6), now.AddDays(6).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(8), now.AddDays(8).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(10), now.AddDays(10).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityFactory.Create(2, new DateRange(now.AddDays(12), now.AddDays(12).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(14), now.AddDays(14).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(16), now.AddDays(16).AddHours(3)), FlatFeeContractEntity.Create(175, PaymentMethod.Cash)),
                OpportunityFactory.Create(3, new DateRange(now.AddDays(18), now.AddDays(18).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Cash)),
                OpportunityFactory.Create(4, new DateRange(now.AddDays(22), now.AddDays(22).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(5, new DateRange(now.AddDays(24), now.AddDays(24).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(6, new DateRange(now.AddDays(26), now.AddDays(26).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(30), now.AddDays(30).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(32), now.AddDays(32).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(34), now.AddDays(34).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(36), now.AddDays(36).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(38), now.AddDays(38).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-60), now.AddDays(-60).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(-90), now.AddDays(-90).AddHours(3)), VersusContractEntity.Create(100, 70, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(120), now.AddDays(120).AddHours(3)), VenueHireContractEntity.Create(250, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(150), now.AddDays(150).AddHours(3)), DoorSplitContractEntity.Create(65, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(180), now.AddDays(180).AddHours(3)), VersusContractEntity.Create(150, 60, PaymentMethod.Cash)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(200), now.AddDays(200).AddHours(3)), FlatFeeContractEntity.Create(150, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(210), now.AddDays(210).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(220), now.AddDays(220).AddHours(3)), VersusContractEntity.Create(100, 70, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), FlatFeeContractEntity.Create(200, PaymentMethod.Transfer)),
                OpportunityFactory.Create(1, new DateRange(now.AddDays(20), now.AddDays(20).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Transfer)),
            };
            context.Opportunities.AddRange(opportunities);
            await context.SaveChangesAsync();
        });

        await context.OpportunityGenres.SeedIfEmptyAsync(async () =>
        {
            var opportunityGenres = new OpportunityGenreEntity[]
            {
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 1, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 2, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 3, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 4, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 5, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 6, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 7, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 8, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 9, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 10, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 11, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 12, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 13, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 14, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 15, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 16, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 17, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 18, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 19, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 20, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 21, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 22, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 23, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 24, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 25, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 26, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 27, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 28, GenreId = 5 },
                new OpportunityGenreEntity { OpportunityId = 29, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 30, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 31, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 32, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 33, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 34, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 35, GenreId = 8 },
                new OpportunityGenreEntity { OpportunityId = 36, GenreId = 6 },
                new OpportunityGenreEntity { OpportunityId = 37, GenreId = 7 },
                new OpportunityGenreEntity { OpportunityId = 38, GenreId = 3 },
                new OpportunityGenreEntity { OpportunityId = 39, GenreId = 1 },
                new OpportunityGenreEntity { OpportunityId = 40, GenreId = 2 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 4 },
                new OpportunityGenreEntity { OpportunityId = 41, GenreId = 8 }
            };
            context.OpportunityGenres.AddRange(opportunityGenres);
            await context.SaveChangesAsync();
        });

        await context.OpportunityApplications.SeedIfEmptyAsync(async () =>
        {
            seedData.ConfirmedBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Ultimate Dance Party", 27m, 160, 140, now.AddDays(2)).Generate());
            seedData.ConfirmedApp = OpportunityApplicationFactory.Accepted(1, 6, seedData.ConfirmedBooking);

            seedData.PostedDoorSplitBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie Wonderland", 25m, 120, 100, now.AddDays(150)).Generate());
            seedData.PostedDoorSplitApp = OpportunityApplicationFactory.Accepted(1, 53, seedData.PostedDoorSplitBooking);

            seedData.PostedVersusBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Funk it up", 20m, 150, 130, now.AddDays(180)).Generate());
            seedData.PostedVersusApp = OpportunityApplicationFactory.Accepted(2, 54, seedData.PostedVersusBooking);

            seedData.PostedFlatFeeBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie it up!", 20m, 150, 130, now.AddDays(-85)).Generate());
            seedData.PostedFlatFeeApp = OpportunityApplicationFactory.Accepted(2, 31, seedData.PostedFlatFeeBooking);

            seedData.PostedVenueHireBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("VenueHire Spectacular", 30m, 200, 180, now.AddDays(-40)).Generate());
            seedData.PostedVenueHireApp = OpportunityApplicationFactory.Accepted(1, 21, seedData.PostedVenueHireBooking);

            seedData.DoorSplitApp = OpportunityApplicationFactory.Create(1, 56);
            seedData.VersusApp = OpportunityApplicationFactory.Create(1, 57);
            seedData.VenueHireApp = OpportunityApplicationFactory.Create(1, 52);
            seedData.FlatFeeApp = OpportunityApplicationFactory.Create(1, 55);

            seedData.AwaitingPaymentBooking = ConcertBookingFactory.AwaitingPayment(ConcertFaker.GetFaker("Awaiting Show", 15m, 100, 80, now.AddDays(3)).Generate());
            seedData.AwaitingPaymentApp = OpportunityApplicationFactory.Accepted(1, 33, seedData.AwaitingPaymentBooking);

            seedData.FinishedDoorSplitBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("DoorSplit Settlement Show", 20m, 100, 99, now.AddDays(-60)).Generate());
            seedData.FinishedDoorSplitApp = OpportunityApplicationFactory.Accepted(1, 50, seedData.FinishedDoorSplitBooking);

            seedData.FinishedVersusBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Versus Settlement Show", 20m, 100, 99, now.AddDays(-90)).Generate());
            seedData.FinishedVersusApp = OpportunityApplicationFactory.Accepted(1, 51, seedData.FinishedVersusBooking);

            seedData.UpcomingFlatFeeBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Upcoming FlatFee Show", 20m, 150, 150, now).Generate());
            seedData.UpcomingFlatFeeApp = OpportunityApplicationFactory.Accepted(2, 58, seedData.UpcomingFlatFeeBooking);

            seedData.UpcomingVenueHireBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Upcoming VenueHire Show", 30m, 200, 200, now).Generate());
            seedData.UpcomingVenueHireApp = OpportunityApplicationFactory.Accepted(1, 59, seedData.UpcomingVenueHireBooking);

            var applications = new OpportunityApplicationEntity[]
            {
                // Apps 1-20: Complete (past concerts)
                OpportunityApplicationFactory.Accepted(1, 1, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Rockin' all Night", 15m, 120, 80, now.AddDays(-58)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 1, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Non Stop Party", 12m, 110, 70, now.AddDays(-55)).Generate())),
                OpportunityApplicationFactory.Accepted(3, 1, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Super Mix", 18m, 130, 100, now.AddDays(-52)).Generate())),
                OpportunityApplicationFactory.Accepted(4, 1, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Hip-Hop till you flip-flop", 10m, 100, 60, now.AddDays(-49)).Generate())),
                OpportunityApplicationFactory.Accepted(1, 2, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Dance the night away", 25m, 140, 110, now.AddDays(-46)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 2, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Dizzy One", 20m, 150, 90, now.AddDays(-43)).Generate())),
                OpportunityApplicationFactory.Accepted(5, 2, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Beers and Boombox", 30m, 170, 150, now.AddDays(-40)).Generate())),
                OpportunityApplicationFactory.Accepted(6, 2, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Rockin' Tonight!", 16m, 130, 100, now.AddDays(-37)).Generate())),
                OpportunityApplicationFactory.Accepted(1, 3, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Groovin' All Night", 14m, 115, 75, now.AddDays(-34)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 3, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Nonstop Vibes", 22m, 135, 100, now.AddDays(-31)).Generate())),
                OpportunityApplicationFactory.Accepted(7, 3, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Electric Dreams", 13m, 125, 85, now.AddDays(-28)).Generate())),
                OpportunityApplicationFactory.Accepted(8, 3, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Beat Drop Frenzy", 11m, 120, 90, now.AddDays(-25)).Generate())),
                OpportunityApplicationFactory.Accepted(1, 4, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Summer Jam", 19m, 140, 110, now.AddDays(-22)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 4, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Midnight Madness", 17m, 135, 105, now.AddDays(-19)).Generate())),
                OpportunityApplicationFactory.Accepted(9, 4, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Like a Boss", 21m, 145, 115, now.AddDays(-16)).Generate())),
                OpportunityApplicationFactory.Accepted(10, 4, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Lights and Sound", 18m, 140, 120, now.AddDays(-13)).Generate())),
                OpportunityApplicationFactory.Accepted(1, 5, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Rhythm Nation", 26m, 155, 130, now.AddDays(-10)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 5, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Bass Drop Party", 15m, 120, 100, now.AddDays(-7)).Generate())),
                OpportunityApplicationFactory.Accepted(11, 5, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Chill & Thrill", 28m, 160, 145, now.AddDays(-4)).Generate())),
                OpportunityApplicationFactory.Accepted(12, 5, ConcertBookingFactory.Complete(ConcertFaker.GetFaker("Vibin' till Night", 24m, 150, 130, now.AddDays(-1)).Generate())),
                // Apps 21-26: Accepted (upcoming concerts)
                seedData.ConfirmedApp,
                OpportunityApplicationFactory.Accepted(2, 6, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Rock Your Soul", 23m, 130, 100, now.AddDays(5)).Generate())),
                OpportunityApplicationFactory.Accepted(13, 6, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Danceaway", 29m, 155, 140, now.AddDays(8)).Generate())),
                OpportunityApplicationFactory.Accepted(14, 6, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Bassline Groove Beats", 10m, 110, 70, now.AddDays(11)).Generate())),
                OpportunityApplicationFactory.Accepted(1, 7, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Once in a Lifetime!", 15m, 125, 90, now.AddDays(14)).Generate())),
                OpportunityApplicationFactory.Accepted(2, 7, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Jungle Fever", 30m, 180, 170, now.AddDays(17)).Generate())),
                // Apps 27-34: Pending (no concert)
                OpportunityApplicationFactory.Create(15, 7),
                OpportunityApplicationFactory.Create(16, 7),
                OpportunityApplicationFactory.Create(1, 8),
                OpportunityApplicationFactory.Create(2, 8),
                OpportunityApplicationFactory.Create(17, 8),
                OpportunityApplicationFactory.Create(18, 8),
                OpportunityApplicationFactory.Create(17, 40),
                OpportunityApplicationFactory.Create(18, 41),
                // App 35: Accepted (upcoming concert)
                OpportunityApplicationFactory.Accepted(1, 14, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Boogie Nights", 20m, 100, 80, now.AddDays(6)).Generate())),
                // Apps 36-38: Pending (no concert)
                OpportunityApplicationFactory.Create(2, 14),
                OpportunityApplicationFactory.Create(3, 14),
                OpportunityApplicationFactory.Create(4, 14),
                // App 39: Accepted (upcoming concert)
                seedData.PostedDoorSplitApp,
                // Apps 40-41: Pending (no concert)
                seedData.DoorSplitApp,
                OpportunityApplicationFactory.Create(7, 15),
                // App 42: Accepted (upcoming concert)
                OpportunityApplicationFactory.Accepted(8, 15, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Bass in the Air", 30m, 140, 120, now.AddDays(18)).Generate())),
                // Apps 43-44: Pending (no concert)
                OpportunityApplicationFactory.Create(9, 16),
                OpportunityApplicationFactory.Create(10, 16),
                // App 45: Accepted (upcoming concert)
                OpportunityApplicationFactory.Accepted(11, 16, ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Jumpin and thumpin", 15m, 100, 80, now.AddDays(22)).Generate())),
                // Apps 46-48: Pending (no concert)
                OpportunityApplicationFactory.Create(12, 16),
                seedData.VersusApp,
                OpportunityApplicationFactory.Create(14, 17),
                // App 49: Accepted (upcoming concert)
                seedData.PostedVersusApp,
                // Apps 50-70: Pending (no concert)
                OpportunityApplicationFactory.Create(16, 17),
                OpportunityApplicationFactory.Create(1, 34),
                OpportunityApplicationFactory.Create(2, 34),
                OpportunityApplicationFactory.Create(19, 34),
                OpportunityApplicationFactory.Create(20, 34),
                OpportunityApplicationFactory.Create(1, 38),
                OpportunityApplicationFactory.Create(2, 38),
                OpportunityApplicationFactory.Create(12, 38),
                OpportunityApplicationFactory.Create(4, 38),
                OpportunityApplicationFactory.Create(1, 45),
                OpportunityApplicationFactory.Create(2, 46),
                OpportunityApplicationFactory.Create(3, 47),
                OpportunityApplicationFactory.Create(4, 48),
                OpportunityApplicationFactory.Create(5, 49),
                OpportunityApplicationFactory.Create(2, 50),
                OpportunityApplicationFactory.Create(2, 51),
                seedData.VenueHireApp,
                OpportunityApplicationFactory.Create(2, 52),
                seedData.FlatFeeApp,
                // App 71: PostedFlatFeeApp (declared before array)
                seedData.PostedFlatFeeApp,
                // Apps 72-75: Pending (no concert)
                OpportunityApplicationFactory.Create(3, 31),
                OpportunityApplicationFactory.Create(1, 32),
                OpportunityApplicationFactory.Create(2, 32),
                OpportunityApplicationFactory.Create(3, 32),
                // App 76: AwaitingPayment (concert 33)
                seedData.AwaitingPaymentApp,
                // App 77: PostedVenueHireApp (concert 34)
                seedData.PostedVenueHireApp,
                // App 78: FinishedDoorSplitApp (concert 35) — VenueId=1, DoorSplit 70%
                seedData.FinishedDoorSplitApp,
                // App 79: FinishedVersusApp (concert 36) — VenueId=1, Versus 100+70%
                seedData.FinishedVersusApp,
                // App 80: UpcomingFlatFeeApp — VenueId=1, FlatFee (future)
                seedData.UpcomingFlatFeeApp,
                // App 81: UpcomingVenueHireApp — VenueId=1, VenueHire (future)
                seedData.UpcomingVenueHireApp,
            };
            context.OpportunityApplications.AddRange(applications);
            await context.SaveChangesAsync();
        });

        await context.ConcertGenres.SeedIfEmptyAsync(async () =>
        {
            var concertGenres = new ConcertGenreEntity[]
            {
                new ConcertGenreEntity { ConcertId = 1, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 1, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 2, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 3, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 4, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 5, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 6, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 7, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 8, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 9, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 10, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 11, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 12, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 13, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 14, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 15, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 16, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 17, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 18, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 19, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 20, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 21, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 22, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 23, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 24, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 25, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 26, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 27, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 6 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 28, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 2 },
                new ConcertGenreEntity { ConcertId = 29, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 8 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 1 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 4 },
                new ConcertGenreEntity { ConcertId = 30, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 31, GenreId = 7 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 3 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 5 },
                new ConcertGenreEntity { ConcertId = 32, GenreId = 7 },
            };
            context.ConcertGenres.AddRange(concertGenres);
            await context.SaveChangesAsync();
        });

        await context.Tickets.SeedIfEmptyAsync(async () =>
        {
            var tickets = new TicketEntity[]
            {
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 1, Array.Empty<byte>(), now.AddDays(-58)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 1, Array.Empty<byte>(), now.AddDays(-57)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 1, Array.Empty<byte>(), now.AddDays(-56)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 2, Array.Empty<byte>(), now.AddDays(-55)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 2, Array.Empty<byte>(), now.AddDays(-54)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 2, Array.Empty<byte>(), now.AddDays(-53)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 3, Array.Empty<byte>(), now.AddDays(-52)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 3, Array.Empty<byte>(), now.AddDays(-51)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[2], 3, Array.Empty<byte>(), now.AddDays(-50)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 4, Array.Empty<byte>(), now.AddDays(-49)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 4, Array.Empty<byte>(), now.AddDays(-48)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 4, Array.Empty<byte>(), now.AddDays(-47)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[2], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 5, Array.Empty<byte>(), now.AddDays(-46)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[2], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 5, Array.Empty<byte>(), now.AddDays(-45)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 5, Array.Empty<byte>(), now.AddDays(-44)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 6, Array.Empty<byte>(), now.AddDays(-43)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 6, Array.Empty<byte>(), now.AddDays(-42)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 7, Array.Empty<byte>(), now.AddDays(-40)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 8, Array.Empty<byte>(), now.AddDays(-38)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 8, Array.Empty<byte>(), now.AddDays(-37)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 9, Array.Empty<byte>(), now.AddDays(-36)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 10, Array.Empty<byte>(), now.AddDays(-34)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 11, Array.Empty<byte>(), now.AddDays(-32)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 12, Array.Empty<byte>(), now.AddDays(-30)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 13, Array.Empty<byte>(), now.AddDays(-28)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 14, Array.Empty<byte>(), now.AddDays(-26)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 15, Array.Empty<byte>(), now.AddDays(-24)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 16, Array.Empty<byte>(), now.AddDays(-22)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[5], 17, Array.Empty<byte>(), now.AddDays(-20)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 18, Array.Empty<byte>(), now.AddDays(-18)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 19, Array.Empty<byte>(), now.AddDays(-16)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 20, Array.Empty<byte>(), now.AddDays(-14)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 21, Array.Empty<byte>(), now.AddDays(-12)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[0], 22, Array.Empty<byte>(), now.AddDays(-10)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 23, Array.Empty<byte>(), now.AddDays(-8)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[3], 24, Array.Empty<byte>(), now.AddDays(-6)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), artistManagerIds[1], 25, Array.Empty<byte>(), now.AddDays(-4)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[1], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[4], 26, Array.Empty<byte>(), now.AddDays(-2)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], seedData.FinishedDoorSplitBooking.Concert!.Id, Array.Empty<byte>(), now.AddDays(-60)),
                TicketFactory.Create(Guid.CreateVersion7(), customerIds[0], seedData.FinishedVersusBooking.Concert!.Id, Array.Empty<byte>(), now.AddDays(-90)),
            };
            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();

            await context.Reviews.SeedIfEmptyAsync(async () =>
            {
                var reviews = new ReviewEntity[]
                {
                    ReviewFactory.Create(tickets[0].Id, 4, "Amazing performance!"),
                    ReviewFactory.Create(tickets[1].Id, 5, "Loved every moment!"),
                    ReviewFactory.Create(tickets[2].Id, 5, "Unforgettable night!"),
                    ReviewFactory.Create(tickets[3].Id, 4, "Great energy from the crowd."),
                    ReviewFactory.Create(tickets[4].Id, 3, "Good, but the sound was a bit off."),
                    ReviewFactory.Create(tickets[5].Id, 5, "Perfect setlist and vibes!"),
                    ReviewFactory.Create(tickets[6].Id, 4, "Would attend again!"),
                    ReviewFactory.Create(tickets[7].Id, 5, "Fantastic indie atmosphere."),
                    ReviewFactory.Create(tickets[8].Id, 4, "Loved the venue!"),
                    ReviewFactory.Create(tickets[9].Id, 4, "Solid performance."),
                    ReviewFactory.Create(tickets[10].Id, 5, "Caught my new favorite artist!"),
                    ReviewFactory.Create(tickets[11].Id, 3, "Good music, but crowded."),
                    ReviewFactory.Create(tickets[12].Id, 5, "Indie dream come true."),
                    ReviewFactory.Create(tickets[13].Id, 4, "Chill night out."),
                    ReviewFactory.Create(tickets[14].Id, 5, "Incredible stage presence!"),
                    ReviewFactory.Create(tickets[15].Id, 4, "Would love to see them again."),
                    ReviewFactory.Create(tickets[16].Id, 5, "Next-level visuals."),
                    ReviewFactory.Create(tickets[17].Id, 4, "Very unique sound."),
                    ReviewFactory.Create(tickets[18].Id, 4, "Great crowd energy."),
                    ReviewFactory.Create(tickets[19].Id, 5, "Absolute fire show."),
                    ReviewFactory.Create(tickets[20].Id, 5, "Perfect DnB experience."),
                    ReviewFactory.Create(tickets[21].Id, 4, "Smooth lyrical vibes."),
                    ReviewFactory.Create(tickets[22].Id, 5, "Top-tier show!"),
                    ReviewFactory.Create(tickets[23].Id, 4, "Nice intimate gig."),
                    ReviewFactory.Create(tickets[24].Id, 3, "A bit too loud but still fun."),
                    ReviewFactory.Create(tickets[25].Id, 4, "Well organized event."),
                    ReviewFactory.Create(tickets[26].Id, 5, "Really enjoyed it."),
                    ReviewFactory.Create(tickets[27].Id, 5, "Brought my friends, all loved it."),
                    ReviewFactory.Create(tickets[28].Id, 3, "Solid but expected more."),
                    ReviewFactory.Create(tickets[29].Id, 4, "The lighting was amazing!"),
                    ReviewFactory.Create(tickets[30].Id, 5, "Instant classic."),
                    ReviewFactory.Create(tickets[31].Id, 4, "Had a great time."),
                    ReviewFactory.Create(tickets[32].Id, 4, "Venue was packed with energy.")
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            });
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
