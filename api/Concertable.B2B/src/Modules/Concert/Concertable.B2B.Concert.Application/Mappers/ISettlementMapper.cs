using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Projections;
using Concertable.B2B.Concert.Application.Responses;

namespace Concertable.B2B.Concert.Application.Mappers;

/// <summary>
/// Builds the manager-facing <see cref="ISettlement"/> for a concert, keyed per <see cref="DealDto.DealType"/>:
/// a fixed gross for FlatFee/VenueHire, the revenue-share formula plus declaration lifecycle for
/// DoorSplit/Guarantee Plus. Callers never branch on the deal — the keyed factory selects the implementation.
/// </summary>
internal interface ISettlementMapper
{
    ISettlement ToSettlement(DealDto deal, ConcertDetails concert, RevenueShareSettlementRowProjection? settlement, DateTime nowUtc);
}
