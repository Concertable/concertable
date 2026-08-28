using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Projections;
using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Responses;

namespace Concertable.B2B.Concert.Application.Mappers;

/// <summary>FlatFee and VenueHire: the payee gross is fixed by the signed terms and known now.</summary>
internal sealed class FixedSettlementMapper : ISettlementMapper
{
    private readonly ISettlementGrossCalculator grossCalculator;

    public FixedSettlementMapper(ISettlementGrossCalculator grossCalculator)
    {
        this.grossCalculator = grossCalculator;
    }

    public ISettlement ToSettlement(DealDto deal, ConcertDetails concert, RevenueShareSettlementRowProjection? settlement, DateTime nowUtc) =>
        new FixedSettlement(grossCalculator.CalculateGross(deal).ToMinorUnits());
}
