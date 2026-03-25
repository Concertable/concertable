using Core.Entities;
using Core.Enums;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public static class TestDbInitializer
{
    public static readonly Guid VenueManagerId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid ArtistManagerId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid CustomerId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    public static readonly Guid AdminId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");

    public static async Task InitializeAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        db.Users.Add(new VenueManagerEntity
        {
            Id = VenueManagerId,
            Email = "venuemanager@test.com",
            PasswordHash = string.Empty,
            Role = Role.VenueManager,
            StripeId = "acct_test_venuemanager",
            Location = new Point(0, 51) { SRID = 4326 }
        });

        db.Users.Add(new ArtistManagerEntity
        {
            Id = ArtistManagerId,
            Email = "artistmanager@test.com",
            PasswordHash = string.Empty,
            Role = Role.ArtistManager,
            Location = new Point(0, 51) { SRID = 4326 }
        });

        db.Users.Add(new CustomerEntity
        {
            Id = CustomerId,
            Email = "customer@test.com",
            PasswordHash = string.Empty,
            Role = Role.Customer,
            Location = new Point(0, 51) { SRID = 4326 }
        });

        db.Users.Add(new UserEntity
        {
            Id = AdminId,
            Email = "admin@test.com",
            PasswordHash = string.Empty,
            Role = Role.Admin,
            Location = new Point(0, 51) { SRID = 4326 }
        });

        db.Venues.Add(new VenueEntity
        {
            UserId = VenueManagerId,
            Name = "Test Venue",
            About = "Test",
            ImageUrl = "test.jpg"
        });

        var genres = new GenreEntity[]
        {
            new GenreEntity { Id = 1, Name = "Rock" },
            new GenreEntity { Id = 2, Name = "Jazz" },
            new GenreEntity { Id = 3, Name = "Hip-Hop" },
            new GenreEntity { Id = 4, Name = "Electronic" }
        };
        db.Genres.AddRange(genres);

        await db.SaveChangesAsync();
    }
}
