using Concertable.B2B.Concert.Application.DTOs;

namespace Concertable.B2B.Concert.Application.Projections;

/// <summary>
/// The one-round-trip manager-details query shape: the marketplace <see cref="ConcertDetails"/> plus the
/// raw revenue-share settlement row (null for a fixed-fee deal, or before the venue declares). The service
/// maps this into a non-nullable <c>ISettlement</c> before returning its own <c>ManagerConcertDetails</c>.
/// </summary>
internal sealed record ManagerConcertDetailsProjection
{
    public required ConcertDetails Concert { get; init; }
    public RevenueShareSettlementRowProjection? Settlement { get; init; }
}

/// <summary>The columns of a concert's revenue-share settlement row — projected whole so absence is one null, not a zero.</summary>
internal sealed record RevenueShareSettlementRowProjection(
    decimal DoorRevenue,
    DateTime DeclaredAtUtc,
    SettlementReview? Review);
