using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Strategies;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class SettlementGrossCalculator : ISettlementGrossCalculator
{
    private readonly IConcertDealStrategyFactory<ISettlementGrossCalculator> calculators;

    public SettlementGrossCalculator(IConcertDealStrategyFactory<ISettlementGrossCalculator> calculators)
    {
        this.calculators = calculators;
    }

    public Money CalculateGross(DealDto deal, Money eligibleTakings) =>
        this.calculators.Create(deal.DealType).CalculateGross(deal, eligibleTakings);
}
