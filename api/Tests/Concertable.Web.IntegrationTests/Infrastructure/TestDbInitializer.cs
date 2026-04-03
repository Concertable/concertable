using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities.Contracts;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Geometry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;

    public TestDbInitializer(ApplicationDbContext context, TimeProvider timeProvider, [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
        this.geometryProvider = geometryProvider;
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
                    PasswordHash = string.Empty,
                    Role = Role.VenueManager,
                    StripeId = "acct_test_venuemanager",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new VenueManagerEntity
                {
                    Id = TestConstants.VenueManager2.Id,
                    Email = "venuemanager2@test.com",
                    PasswordHash = string.Empty,
                    Role = Role.VenueManager,
                    StripeId = "acct_test_venuemanager2",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new ArtistManagerEntity
                {
                    Id = TestConstants.ArtistManager.Id,
                    Email = "artistmanager1@test.com",
                    PasswordHash = string.Empty,
                    Role = Role.ArtistManager,
                    StripeId = "acct_test_artistmanager",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new CustomerEntity
                {
                    Id = TestConstants.Customer.Id,
                    Email = "customer@test.com",
                    PasswordHash = string.Empty,
                    Role = Role.Customer,
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new UserEntity
                {
                    Id = TestConstants.Admin.Id,
                    Email = "admin@test.com",
                    PasswordHash = string.Empty,
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
                ImageUrl = "artist.jpg",
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
                ImageUrl = "test.jpg",
                Opportunities =
                [
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(2),
                        EndDate = DateTime.UtcNow.AddMonths(2).AddHours(3),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(3),
                        EndDate = DateTime.UtcNow.AddMonths(3).AddHours(3),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(4),
                        EndDate = DateTime.UtcNow.AddMonths(4).AddHours(3),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(5),
                        EndDate = DateTime.UtcNow.AddMonths(5).AddHours(3),
                        Contract = new VersusContractEntity { Guarantee = 200, ArtistDoorPercent = 50 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(6),
                        EndDate = DateTime.UtcNow.AddMonths(6).AddHours(3),
                        Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(7),
                        EndDate = DateTime.UtcNow.AddMonths(7).AddHours(3),
                        Contract = new VenueHireContractEntity { HireFee = 300 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(8),
                        EndDate = DateTime.UtcNow.AddMonths(8).AddHours(3),
                        Contract = new FlatFeeContractEntity { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(9),
                        EndDate = DateTime.UtcNow.AddMonths(9).AddHours(3),
                        Contract = new DoorSplitContractEntity { ArtistDoorPercent = 70 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(10),
                        EndDate = DateTime.UtcNow.AddMonths(10).AddHours(3),
                        Contract = new VersusContractEntity { Guarantee = 200, ArtistDoorPercent = 50 },
                        OpportunityGenres = [new OpportunityGenreEntity { GenreId = TestConstants.GenreId }]
                    },
                    new OpportunityEntity
                    {
                        StartDate = DateTime.UtcNow.AddMonths(11),
                        EndDate = DateTime.UtcNow.AddMonths(11).AddHours(3),
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
                    Status = ApplicationStatus.Settled,
                    Concert = new ConcertEntity
                    {
                        Name = "Draft Concert",
                        About = "Draft Concert About",
                        Price = 0,
                        TotalTickets = 0,
                        AvailableTickets = 0
                    }
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.AwaitingPayment.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.AwaitingPayment,
                    Concert = new ConcertEntity
                    {
                        Name = "Unsettled Concert",
                        About = "Unsettled Concert About",
                        Price = 0,
                        TotalTickets = 0,
                        AvailableTickets = 0
                    }
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
                    Status = ApplicationStatus.Settled,
                    Concert = new ConcertEntity
                    {
                        Name = "Posted FlatFee Concert",
                        About = "Posted FlatFee Concert About",
                        Price = 10.00m,
                        TotalTickets = 100,
                        AvailableTickets = 100,
                        DatePosted = DateTime.UtcNow
                    }
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedDoorSplit.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Settled,
                    Concert = new ConcertEntity
                    {
                        Name = "Posted DoorSplit Concert",
                        About = "Posted DoorSplit Concert About",
                        Price = 10.00m,
                        TotalTickets = 100,
                        AvailableTickets = 100,
                        DatePosted = DateTime.UtcNow
                    }
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedVersus.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Settled,
                    Concert = new ConcertEntity
                    {
                        Name = "Posted Versus Concert",
                        About = "Posted Versus Concert About",
                        Price = 10.00m,
                        TotalTickets = 100,
                        AvailableTickets = 100,
                        DatePosted = DateTime.UtcNow
                    }
                },
                new OpportunityApplicationEntity
                {
                    OpportunityId = TestConstants.PostedVenueHire.OpportunityId,
                    ArtistId = TestConstants.ArtistId,
                    Status = ApplicationStatus.Settled,
                    Concert = new ConcertEntity
                    {
                        Name = "Posted VenueHire Concert",
                        About = "Posted VenueHire Concert About",
                        Price = 10.00m,
                        TotalTickets = 100,
                        AvailableTickets = 100,
                        DatePosted = DateTime.UtcNow
                    }
                }
            );

            await context.SaveChangesAsync();
        }
    }

}