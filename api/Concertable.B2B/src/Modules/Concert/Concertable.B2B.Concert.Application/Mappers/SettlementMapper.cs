using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Projections;
using Concertable.B2B.Concert.Application.Responses;
using Concertable.B2B.Concert.Application.Strategies;

namespace Concertable.B2B.Concert.Application.Mappers;

internal sealed class SettlementMapper : ISettlementMapper
{
    private readonly IConcertDealStrategyFactory<ISettlementMapper> mappers;

    public SettlementMapper(IConcertDealStrategyFactory<ISettlementMapper> mappers)
    {
        this.mappers = mappers;
    }

    public ISettlement ToSettlement(DealDto deal, ConcertDetails concert, RevenueShareSettlementRowProjection? settlement, DateTime nowUtc) =>
        mappers.Create(deal.DealType).ToSettlement(deal, concert, settlement, nowUtc);
}
