using Application.Interfaces;
using Application.Interfaces.Geometry;
using Concertable.Core.Entities.Contracts;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;
    private readonly IGeometryProvider geometryProvider;

    public TestDbInitializer(ApplicationDbContext context, TimeProvider timeProvider, IGeometryProvider geometryProvider)
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

        var rockGenreId = await context.Genres
    .Where(g => g.Name == "Rock")
    .Select(g => g.Id)
    .FirstAsync();

        if (!await context.Users.AnyAsync())
        {
            context.Users.AddRange(
                new VenueManagerEntity
                {
                    Id = TestConstants.VenueManager.Id,
                    Email = "venuemanager@test.com",
                    PasswordHash = string.Empty,
                    Role = Role.VenueManager,
                    StripeId = "acct_test_venuemanager",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new VenueManagerEntity
                {
                    Id = TestConstants.VenueManager2.Id,
                    Email = "venuemanager_b@test.com",
                    PasswordHash = string.Empty,
                    Role = Role.VenueManager,
                    StripeId = "acct_test_venuemanager2",
                    Location = geometryProvider.CreatePoint(51, 0)
                },
                new ArtistManagerEntity
                {
                    Id = TestConstants.ArtistManager.Id,
                    Email = "artistmanager@test.com",
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
            var opportunity = new ConcertOpportunityEntity
            {
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(2).AddHours(3),
                Contract = new FlatFeeContractEntity
                {
                    PaymentMethod = PaymentMethod.Cash,
                    Fee = 500
                },
                OpportunityGenres =
                [
                    new OpportunityGenreEntity { GenreId = TestConstants.GenreId }
                ]
            };

            var venue = new VenueEntity
            {
                UserId = TestConstants.VenueManager.Id,
                Name = "Test Venue",
                About = "Test",
                ImageUrl = "test.jpg",
                Opportunities = [opportunity]
            };

            context.Venues.Add(venue);
            await context.SaveChangesAsync();
        }

        if (!await context.ConcertApplications.AnyAsync())
        {
            context.ConcertApplications.Add(new ConcertApplicationEntity
            {
                OpportunityId = 1,
                ArtistId = 1,
                Status = ApplicationStatus.Pending
            });

            await context.SaveChangesAsync();
        }
    }

}