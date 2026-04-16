using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Auth;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities.Contracts;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Data.SeedData;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Core.ValueObjects;
using Concertable.Infrastructure.Services.Geometry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;
    private readonly IPasswordHasher passwordHasher;

    public TestDbInitializer(ApplicationDbContext context, TimeProvider timeProvider, [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider, IPasswordHasher passwordHasher)
    {
        this.context = context;
        this.timeProvider = timeProvider;
        this.geometryProvider = geometryProvider;
        this.passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();

        if (!await context.Genres.AnyAsync())
        {
            context.Genres.AddRange(
                new GenreEntity { Name = "Rock" },
                new GenreEntity { Name = "Jazz" },
                new GenreEntity { Name = "Hip-Hop" },
                new GenreEntity { Name = "Electronic" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            context.Users.AddRange(
                new VenueManagerEntity
                {
                    Id = TestConstants.VenueManager.Id,
                    Email = "venuemanager1@test.com",
                    PasswordHash = passwordHasher.Hash(TestConstants.TestPassword),
                    IsEmailVerified = true,
                    Role = Role.VenueManager,
                    StripeAccountId = "acct_test_venuemanager",
                    StripeCustomerId = "cus_test_venuemanager",
                    Address = new Address(LocationList.GetLocations()[0].County, LocationList.GetLocations()[0].Town),
                    Location = geometryProvider.CreatePoint(LocationList.GetLocations()[0].Latitude, LocationList.GetLocations()[0].Longitude)
                },
                new VenueManagerEntity
                {
                    Id = TestConstants.VenueManager2.Id,
                    Email = "venuemanager2@test.com",
                    PasswordHash = passwordHasher.Hash(TestConstants.TestPassword),
                    IsEmailVerified = true,
                    Role = Role.VenueManager,
                    StripeAccountId = "acct_test_venuemanager2",
                    StripeCustomerId = "cus_test_venuemanager2",
                    Address = new Address(LocationList.GetLocations()[1].County, LocationList.GetLocations()[1].Town),
                    Location = geometryProvider.CreatePoint(LocationList.GetLocations()[1].Latitude, LocationList.GetLocations()[1].Longitude)
                },
                new ArtistManagerEntity
                {
                    Id = TestConstants.ArtistManager.Id,
                    Email = "artistmanager1@test.com",
                    PasswordHash = passwordHasher.Hash(TestConstants.TestPassword),
                    IsEmailVerified = true,
                    Role = Role.ArtistManager,
                    StripeAccountId = "acct_test_artistmanager",
                    StripeCustomerId = "cus_test_artistmanager",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new CustomerEntity
                {
                    Id = TestConstants.Customer.Id,
                    Email = "customer@test.com",
                    PasswordHash = passwordHasher.Hash(TestConstants.TestPassword),
                    IsEmailVerified = true,
                    Role = Role.Customer,
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new UserEntity
                {
                    Id = TestConstants.Admin.Id,
                    Email = "admin@test.com",
                    PasswordHash = passwordHasher.Hash(TestConstants.TestPassword),
                    IsEmailVerified = true,
                    Role = Role.Admin,
                    Location = geometryProvider.CreatePoint(51, 0)
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Artists.AnyAsync())
        {
            context.Artists.Add(ArtistEntity.Create(
                TestConstants.ArtistManager.Id,
                "Test Artist",
                "Test Artist About",
                "artist.jpg",
                [TestConstants.GenreId]));
            await context.SaveChangesAsync();
        }

        if (!await context.Venues.AnyAsync())
        {
            var venue = VenueEntity.Create(TestConstants.VenueManager.Id, "Test Venue", "Test", "test.jpg");

            var opportunities = new[]
            {
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(2), DateTime.UtcNow.AddMonths(2).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(3), DateTime.UtcNow.AddMonths(3).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(4), DateTime.UtcNow.AddMonths(4).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(5), DateTime.UtcNow.AddMonths(5).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(6), DateTime.UtcNow.AddMonths(6).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(7), DateTime.UtcNow.AddMonths(7).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(8), DateTime.UtcNow.AddMonths(8).AddHours(3)), FlatFeeContractEntity.Create(500, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(9), DateTime.UtcNow.AddMonths(9).AddHours(3)), DoorSplitContractEntity.Create(70, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(10), DateTime.UtcNow.AddMonths(10).AddHours(3)), VersusContractEntity.Create(200, 50, PaymentMethod.Cash), [TestConstants.GenreId]),
                OpportunityEntity.Create(1, new DateRange(DateTime.UtcNow.AddMonths(11), DateTime.UtcNow.AddMonths(11).AddHours(3)), VenueHireContractEntity.Create(300, PaymentMethod.Cash), [TestConstants.GenreId]),
            };

            foreach (var opportunity in opportunities)
                venue.Opportunities.Add(opportunity);

            context.Venues.Add(venue);
            await context.SaveChangesAsync();
        }

        if (!await context.OpportunityApplications.AnyAsync())
        {
            var draftConcert = ConcertEntity.CreateDraft(0, "Draft Concert", "Draft Concert About", []);

            var unsettledConcert = ConcertEntity.CreateDraft(0, "Unsettled Concert", "Unsettled Concert About", []);

            var postedFlatFeeConcert = ConcertEntity.CreateDraft(0, "Posted FlatFee Concert", "Posted FlatFee Concert About", []);
            postedFlatFeeConcert.Post("Posted FlatFee Concert", "Posted FlatFee Concert About", 10.00m, 100, DateTime.UtcNow);

            var postedDoorSplitConcert = ConcertEntity.CreateDraft(0, "Posted DoorSplit Concert", "Posted DoorSplit Concert About", []);
            postedDoorSplitConcert.Post("Posted DoorSplit Concert", "Posted DoorSplit Concert About", 10.00m, 100, DateTime.UtcNow);

            var postedVersusConcert = ConcertEntity.CreateDraft(0, "Posted Versus Concert", "Posted Versus Concert About", []);
            postedVersusConcert.Post("Posted Versus Concert", "Posted Versus Concert About", 10.00m, 100, DateTime.UtcNow);

            var postedVenueHireConcert = ConcertEntity.CreateDraft(0, "Posted VenueHire Concert", "Posted VenueHire Concert About", []);
            postedVenueHireConcert.Post("Posted VenueHire Concert", "Posted VenueHire Concert About", 10.00m, 100, DateTime.UtcNow);

            var flatFeeApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.FlatFee.OpportunityId);

            var settledApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.Settled.OpportunityId);
            settledApp.Accept(draftConcert);

            var awaitingPaymentApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.AwaitingPayment.OpportunityId);
            awaitingPaymentApp.Accept(unsettledConcert);
            awaitingPaymentApp.AwaitPayment();

            var versusApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.Versus.OpportunityId);

            var doorSplitApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.DoorSplit.OpportunityId);

            var venueHireApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.VenueHire.OpportunityId);

            var postedFlatFeeApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.PostedFlatFee.OpportunityId);
            postedFlatFeeApp.Accept(postedFlatFeeConcert);

            var postedDoorSplitApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.PostedDoorSplit.OpportunityId);
            postedDoorSplitApp.Accept(postedDoorSplitConcert);

            var postedVersusApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.PostedVersus.OpportunityId);
            postedVersusApp.Accept(postedVersusConcert);

            var postedVenueHireApp = OpportunityApplicationEntity.Create(TestConstants.ArtistId, TestConstants.PostedVenueHire.OpportunityId);
            postedVenueHireApp.Accept(postedVenueHireConcert);

            context.OpportunityApplications.AddRange(
                flatFeeApp,
                settledApp,
                awaitingPaymentApp,
                versusApp,
                doorSplitApp,
                venueHireApp,
                postedFlatFeeApp,
                postedDoorSplitApp,
                postedVersusApp,
                postedVenueHireApp
            );

            await context.SaveChangesAsync();
        }
    }

}