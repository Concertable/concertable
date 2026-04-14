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
            context.Artists.Add(new ArtistEntity
            {
                UserId = TestConstants.ArtistManager.Id,
                Name = "Test Artist",
                About = "Test Artist About",
                BannerUrl = "artist.jpg",
                ArtistGenres =
                [
                    new ArtistGenreEntity { GenreId = TestConstants.GenreId }
                ]
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Venues.AnyAsync())
        {
            var venue = new VenueEntity
            {
                UserId = TestConstants.VenueManager.Id,
                Name = "Test Venue",
                About = "Test",
                BannerUrl = "test.jpg",
                Opportunities =
                [
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(2), DateTime.UtcNow.AddMonths(2).AddHours(3)),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(3), DateTime.UtcNow.AddMonths(3).AddHours(3)),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(4), DateTime.UtcNow.AddMonths(4).AddHours(3)),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(5), DateTime.UtcNow.AddMonths(5).AddHours(3)),
                        Contract = new VersusContractEntity { Guarantee = 200, ArtistDoorPercent = 50 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(6), DateTime.UtcNow.AddMonths(6).AddHours(3)),
                        Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(7), DateTime.UtcNow.AddMonths(7).AddHours(3)),
                        Contract = new VenueHireContractEntity { HireFee = 300 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(8), DateTime.UtcNow.AddMonths(8).AddHours(3)),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(9), DateTime.UtcNow.AddMonths(9).AddHours(3)),
                        Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(10), DateTime.UtcNow.AddMonths(10).AddHours(3)),
                        Contract = new VersusContractEntity { Guarantee = 200, ArtistDoorPercent = 50 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        Period = new DateRange(DateTime.UtcNow.AddMonths(11), DateTime.UtcNow.AddMonths(11).AddHours(3)),
                        Contract = new VenueHireContractEntity { HireFee = 300 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    }
                ]
            };

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

            context.OpportunityApplications.AddRange(
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.FlatFee.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Pending
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.Settled.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Accepted,
                    Concert = draftConcert
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.AwaitingPayment.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.AwaitingPayment,
                    Concert = unsettledConcert
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.Versus.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Pending
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.DoorSplit.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Pending
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.VenueHire.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Pending
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedFlatFee.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Accepted,
                    Concert = postedFlatFeeConcert
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedDoorSplit.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Accepted,
                    Concert = postedDoorSplitConcert
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedVersus.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Accepted,
                    Concert = postedVersusConcert
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedVenueHire.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Accepted,
                    Concert = postedVenueHireConcert
                }
            );

            await context.SaveChangesAsync();
        }
    }

}