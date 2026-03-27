using Core.Enums;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public record TestUser(Guid Id, Role Role);

public static class TestConstants
{
    public const int ArtistId = 1;
    public const int GenreId = 1;
    public const int VenueId = 1;

    public static class FlatFee
    {
        public const int OpportunityId = 1;
        public const int ApplicationId = 1;
    }

    public static class Settled
    {
        public const int OpportunityId = 2;
        public const int ApplicationId = 2;
        public const int ConcertId = 1;
    }

    public static class AwaitingPayment
    {
        public const int OpportunityId = 3;
        public const int ApplicationId = 3;
        public const int ConcertId = 2;
    }

    public static readonly TestUser VenueManager = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), Role.VenueManager);
    public static readonly TestUser VenueManager2 = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), Role.VenueManager);
    public static readonly TestUser ArtistManager = new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), Role.ArtistManager);
    public static readonly TestUser Customer = new(Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Role.Customer);
    public static readonly TestUser Admin = new(Guid.Parse("dddddddd-0000-0000-0000-000000000001"), Role.Admin);
}
