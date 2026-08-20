using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Strategies;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class SettlementAmountResolver : ISettlementAmountResolver
{
    private readonly IConcertRepository concertRepository;
    private readonly IConcertDealStrategyFactory<ISettlementGrossCalculator> calculators;

    public SettlementAmountResolver(
        IConcertRepository concertRepository,
        IConcertDealStrategyFactory<ISettlementGrossCalculator> calculators)
    {
        this.concertRepository = concertRepository;
        this.calculators = calculators;
    }

    public async Task<Money> ResolveGrossAsync(int concertId, IDeal deal, CancellationToken ct = default)
    {
        var calculator = calculators.Create(deal.DealType);
        var eligibleTakings = calculator.RequiresEligibleTakings
            ? await concertRepository.GetTotalRevenueByConcertIdAsync(concertId)
            : null;
        return calculator.CalculateGross(deal, eligibleTakings);
    }
}
