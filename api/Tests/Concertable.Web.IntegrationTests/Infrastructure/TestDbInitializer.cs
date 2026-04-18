using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.ValueObjects;
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
    private readonly IPasswordHasher passwordHasher;
    private readonly SeedData seed;
    private readonly ILocationFaker locationFaker;
    private readonly TimeProvider timeProvider;

    public TestDbInitializer(
        ApplicationDbContext context,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        IPasswordHasher passwordHasher,
        SeedData seed,
        ILocationFaker locationFaker,
        TimeProvider timeProvider)
    {
        this.context = context;
        this.geometryProvider = geometryProvider;
        this.passwordHasher = passwordHasher;
        this.seed = seed;
        this.locationFaker = locationFaker;
        this.timeProvider = timeProvider;
    }

    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        await context.Genres.SeedIfEmptyAsync(async () =>
        {
            seed.Rock = new GenreEntity { Name = "Rock" };
            seed.Jazz = new GenreEntity { Name = "Jazz" };
            seed.HipHop = new GenreEntity { Name = "Hip-Hop" };
            seed.Electronic = new GenreEntity { Name = "Electronic" };

            context.Genres.AddRange(seed.Rock, seed.Jazz, seed.HipHop, seed.Electronic);
            await context.SaveChangesAsync();
        });

        await context.Users.SeedIfEmptyAsync(async () =>
        {
            var hash = passwordHasher.Hash(SeedData.TestPassword);

            var vm1Loc = locationFaker.Next();
            seed.VenueManager1 = VenueManagerEntity.Create("venuemanager1@test.com", hash);
            seed.VenueManager1.VerifyEmail();
            seed.VenueManager1.StripeAccountId = "acct_test_venuemanager";
            seed.VenueManager1.StripeCustomerId = "cus_test_venuemanager";
            seed.VenueManager1.Location = geometryProvider.CreatePoint(vm1Loc.Latitude, vm1Loc.Longitude);
            seed.VenueManager1.Address = new Address(vm1Loc.County, vm1Loc.Town);

            var vm2Loc = locationFaker.Next();
            seed.VenueManager2 = VenueManagerEntity.Create("venuemanager2@test.com", hash);
            seed.VenueManager2.VerifyEmail();
            seed.VenueManager2.StripeAccountId = "acct_test_venuemanager2";
            seed.VenueManager2.StripeCustomerId = "cus_test_venuemanager2";
            seed.VenueManager2.Location = geometryProvider.CreatePoint(vm2Loc.Latitude, vm2Loc.Longitude);
            seed.VenueManager2.Address = new Address(vm2Loc.County, vm2Loc.Town);

            seed.ArtistManager = ArtistManagerEntity.Create("artistmanager1@test.com", hash);
            seed.ArtistManager.VerifyEmail();
            seed.ArtistManager.StripeAccountId = "acct_test_artistmanager";
            seed.ArtistManager.StripeCustomerId = "cus_test_artistmanager";
            seed.ArtistManager.Location = geometryProvider.CreatePoint(51, 0);

            seed.Customer = CustomerEntity.Create("customer@test.com", hash);
            seed.Customer.VerifyEmail();
            seed.Customer.Location = geometryProvider.CreatePoint(51, 0);

            seed.Admin = AdminEntity.Create("admin@test.com", hash);
            seed.Admin.VerifyEmail();
            seed.Admin.Location = geometryProvider.CreatePoint(51, 0);

            context.Users.AddRange(
                seed.VenueManager1,
                seed.VenueManager2,
                seed.ArtistManager,
                seed.Customer,
                seed.Admin);

            await context.SaveChangesAsync();
        });

        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            seed.Artist = ArtistEntity.Create(
                seed.ArtistManager.Id,
                "Test Artist",
                "Test Artist About",
                "artist.jpg",
                [seed.Rock.Id]);

            context.Artists.Add(seed.Artist);
            await context.SaveChangesAsync();
        });

        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            seed.Venue = VenueEntity.Create(
                seed.VenueManager1.Id,
                "Test Venue",
                "Test Venue About",
                "venue.jpg");

            context.Venues.Add(seed.Venue);
            await context.SaveChangesAsync();
        });

        await context.Opportunities.SeedIfEmptyAsync(async () =>
        {
            seed.Opportunities =
            [
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(2), now.AddMonths(2).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(3), now.AddMonths(3).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(4), now.AddMonths(4).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(5), now.AddMonths(5).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(6), now.AddMonths(6).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(7), now.AddMonths(7).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(8), now.AddMonths(8).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(9), now.AddMonths(9).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(10), now.AddMonths(10).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [seed.Rock.Id]),
                OpportunityEntity.Create(seed.Venue.Id, new DateRange(now.AddMonths(11), now.AddMonths(11).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [seed.Rock.Id]),
            ];

            context.Opportunities.AddRange(seed.Opportunities);
            await context.SaveChangesAsync();
        });

        await context.OpportunityApplications.SeedIfEmptyAsync(async () =>
        {
            var opps = seed.Opportunities;

            seed.FlatFeeApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[0].Id);
            seed.SettledApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[1].Id, "Draft Concert", "Draft Concert About", []);
            seed.AwaitingPaymentApp = OpportunityApplicationFactory.AwaitingPayment(seed.Artist.Id, opps[2].Id, "Unsettled Concert", "Unsettled Concert About", []);
            seed.VersusApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[3].Id);
            seed.DoorSplitApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[4].Id);
            seed.VenueHireApp = OpportunityApplicationFactory.Create(seed.Artist.Id, opps[5].Id);
            seed.PostedFlatFeeApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[6].Id, "Posted FlatFee Concert", "Posted FlatFee Concert About", [], 10.00m, 100, now);
            seed.PostedDoorSplitApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[7].Id, "Posted DoorSplit Concert", "Posted DoorSplit Concert About", [], 10.00m, 100, now);
            seed.PostedVersusApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[8].Id, "Posted Versus Concert", "Posted Versus Concert About", [], 10.00m, 100, now);
            seed.PostedVenueHireApp = OpportunityApplicationFactory.Accepted(seed.Artist.Id, opps[9].Id, "Posted VenueHire Concert", "Posted VenueHire Concert About", [], 10.00m, 100, now);

            context.OpportunityApplications.AddRange(
                seed.FlatFeeApp,
                seed.SettledApp,
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
