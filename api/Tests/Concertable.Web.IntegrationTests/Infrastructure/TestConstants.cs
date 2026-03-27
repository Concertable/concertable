using Core.Enums;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public record TestUser(Guid Id, Role Role);
public record PendingContract(int OpportunityId, int ApplicationId);
public record SettledContract(int OpportunityId, int ApplicationId, int ConcertId);
public record AwaitingPaymentContract(int OpportunityId, int ApplicationId, int ConcertId);
public record CompletedContract(int OpportunityId, int ApplicationId, int ConcertId);

public static class TestConstants
{
    public const int ArtistId = 1;
    public const int GenreId = 1;
    public const int VenueId = 1;

    public static readonly PendingContract FlatFee = new(1, 1);
    public static readonly SettledContract Settled = new(2, 2, 1);
    public static readonly AwaitingPaymentContract AwaitingPayment = new(3, 3, 2);
    public static readonly PendingContract Versus = new(4, 4);
    public static readonly PendingContract DoorSplit = new(5, 5);
    public static readonly PendingContract VenueHire = new(6, 6);

    public static readonly TestUser VenueManager = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), Role.VenueManager);
    public static readonly TestUser VenueManager2 = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), Role.VenueManager);
    public static readonly TestUser ArtistManager = new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), Role.ArtistManager);
    public static readonly TestUser Customer = new(Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Role.Customer);
    public static readonly TestUser Admin = new(Guid.Parse("dddddddd-0000-0000-0000-000000000001"), Role.Admin);
}
