using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Data;
using Concertable.Seeding.Fakers;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Seeding.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly IGeometryProvider geometryProvider;
    private readonly SeedData seed;
    private readonly ILocationFaker locationFaker;
    private readonly TimeProvider timeProvider;
    private readonly IEnumerable<ITestSeeder> seeders;

    public TestDbInitializer(
        ApplicationDbContext context,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        SeedData seed,
        ILocationFaker locationFaker,
        TimeProvider timeProvider,
        IEnumerable<ITestSeeder> seeders)
    {
        this.context = context;
        this.geometryProvider = geometryProvider;
        this.seed = seed;
        this.locationFaker = locationFaker;
        this.timeProvider = timeProvider;
        this.seeders = seeders;
    }

    public async Task InitializeAsync()
    {
        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.MigrateAsync();

        await context.Database.MigrateAsync();

        // Genres are reference data used by module seeders (ArtistTestSeeder needs seed.Rock),
        // so they must be seeded before the SeedAsync loop runs.
        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            seed.Rock = GenreFactory.Create("Rock");
            seed.Jazz = GenreFactory.Create("Jazz");
            seed.HipHop = GenreFactory.Create("Hip-Hop");
            seed.Electronic = GenreFactory.Create("Electronic");

            context.Genres.AddRange(seed.Rock, seed.Jazz, seed.HipHop, seed.Electronic);
            await context.SaveChangesAsync();
        });

        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            seed.Opportunities =
            [
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(2), now.AddMonths(2).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(3), now.AddMonths(3).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(4), now.AddMonths(4).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(5), now.AddMonths(5).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(6), now.AddMonths(6).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(7), now.AddMonths(7).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(8), now.AddMonths(8).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(9), now.AddMonths(9).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(10), now.AddMonths(10).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityFactory.Create(seed.Venue.Id, new DateRange(now.AddMonths(11), now.AddMonths(11).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [seed.Rock.Id]),
            ];

            context.Opportunities.AddRange(seed.Opportunities);
            await context.SaveChangesAsync();
        });

        await context.OpportunityApplications.SeedIfEmptyAsync(async () =>
        {
            var opps = seed.Opportunities;

            seed.FlatFeeApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[0].Id);

            seed.ConfirmedBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Draft Concert", 0m, 100, 100).Generate());
            seed.ConfirmedApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[1].Id, seed.ConfirmedBooking);

            seed.AwaitingPaymentBooking = ConcertBookingFactory.AwaitingPayment(ConcertFaker.GetFaker("Unsettled Concert", 0m, 100, 100).Generate());
            seed.AwaitingPaymentApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[2].Id, seed.AwaitingPaymentBooking);

            seed.VersusApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[3].Id);
            seed.DoorSplitApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[4].Id);
            seed.VenueHireApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[5].Id);

            seed.PostedFlatFeeBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted FlatFee Concert", 10.00m, 100, 100, now).Generate());
            seed.PostedFlatFeeBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedFlatFeeApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[6].Id, seed.PostedFlatFeeBooking);

            seed.PostedDoorSplitBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted DoorSplit Concert", 10.00m, 100, 100, now).Generate());
            seed.PostedDoorSplitBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedDoorSplitApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[7].Id, seed.PostedDoorSplitBooking);

            seed.PostedVersusBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted Versus Concert", 10.00m, 100, 100, now).Generate());
            seed.PostedVersusBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedVersusApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[8].Id, seed.PostedVersusBooking);

            seed.PostedVenueHireBooking = ConcertBookingFactory.Confirmed(ConcertFaker.GetFaker("Posted VenueHire Concert", 10.00m, 100, 100, now).Generate());
            seed.PostedVenueHireBooking.Concert!.ConcertGenres.Add(new ConcertGenreEntity { GenreId = seed.Rock.Id });
            seed.PostedVenueHireApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[9].Id, seed.PostedVenueHireBooking);

            context.OpportunityApplications.AddRange(
                seed.FlatFeeApp,
                seed.ConfirmedApp,
                seed.AwaitingPaymentApp,
                seed.VersusApp,
                seed.DoorSplitApp,
                seed.VenueHireApp,
                seed.PostedFlatFeeApp,
                seed.PostedDoorSplitApp,
                seed.PostedVersusApp,
                seed.PostedVenueHireApp);

            await context.SaveChangesAsync();
        });
    }
}
