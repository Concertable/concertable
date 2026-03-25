using Application.Interfaces.Geometry;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public class TestDbInitializer
{
    public static readonly Guid VenueManagerId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid ArtistManagerId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid CustomerId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    public static readonly Guid AdminId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");

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

        context.Users.Add(new VenueManagerEntity
        {
            Id = VenueManagerId,
            Email = "venuemanager@test.com",
            PasswordHash = string.Empty,
            Role = Role.VenueManager,
            StripeId = "acct_test_venuemanager",
            Location = geometryProvider.CreatePoint(51, 0)
        });

        context.Users.Add(new ArtistManagerEntity
        {
            Id = ArtistManagerId,
            Email = "artistmanager@test.com",
            PasswordHash = string.Empty,
            Role = Role.ArtistManager,
            Location = geometryProvider.CreatePoint(51, 0)
        });

        context.Users.Add(new CustomerEntity
        {
            Id = CustomerId,
            Email = "customer@test.com",
            PasswordHash = string.Empty,
            Role = Role.Customer,
            Location = geometryProvider.CreatePoint(51, 0)
        });

        context.Users.Add(new UserEntity
        {
            Id = AdminId,
            Email = "admin@test.com",
            PasswordHash = string.Empty,
            Role = Role.Admin,
            Location = geometryProvider.CreatePoint(51, 0)
        });

        context.Venues.Add(new VenueEntity
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
        context.Genres.AddRange(genres);

        await context.SaveChangesAsync();
    }
}
