using Core.Enums;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public record TestUser(Guid Id, Role Role);

public static class TestConstants
{
    public static readonly TestUser VenueManager = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), Role.VenueManager);
    public static readonly TestUser VenueManager2 = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), Role.VenueManager);
    public static readonly TestUser ArtistManager = new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), Role.ArtistManager);
    public static readonly TestUser Customer = new(Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Role.Customer);
    public static readonly TestUser Admin = new(Guid.Parse("dddddddd-0000-0000-0000-000000000001"), Role.Admin);

    public const int RockGenreId = 1;
}
