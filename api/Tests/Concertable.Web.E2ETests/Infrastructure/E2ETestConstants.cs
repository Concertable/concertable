using Concertable.Core.Enums;

namespace Concertable.Web.E2ETests.Infrastructure;

public record TestUser(Guid Id, string Email, Role Role);
public record TestConcert(int ConcertId, int ApplicationId, string Name);

public static class E2ETestConstants
{
    public const string TestPassword = "Password11!";

    public static readonly TestUser Customer1 = new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), "customer1@test.com", Role.Customer);
    public static readonly TestUser ArtistManager1 = new(Guid.Parse("cccccccc-0000-0000-0000-000000000001"), "artistmanager1@test.com", Role.ArtistManager);
    public static readonly TestUser VenueManager1 = new(Guid.Parse("dddddddd-0000-0000-0000-000000000001"), "venuemanager1@test.com", Role.VenueManager);

    public static readonly TestConcert PostedConcert = new(ConcertId: 33, ApplicationId: 59, Name: "Dev FlatFee Test Concert");
}
