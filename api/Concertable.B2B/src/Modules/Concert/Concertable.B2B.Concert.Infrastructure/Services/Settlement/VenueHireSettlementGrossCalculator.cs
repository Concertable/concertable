using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class VenueHireSettlementGrossCalculator : ISettlementGrossCalculator
{
    public Money CalculateGross(DealDto deal, Money eligibleTakings) =>
        Money.Gbp(((VenueHireDealDto)deal).HireFee);
}
