using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal abstract class RevenueShareGrossCalculator : ISettlementGrossCalculator
{
    public bool RequiresEligibleTakings => true;

    public Money CalculateGross(IDeal deal, decimal? eligibleTakings)
    {
        var takings = eligibleTakings
            ?? throw new InvalidOperationException(
                $"A {deal.DealType} settlement gross was requested with no eligible takings — the completion gate should make this unreachable.");
        return SettlementGross.InGbp(GrossPounds(deal, takings));
    }

    protected abstract decimal GrossPounds(IDeal deal, decimal eligibleTakings);
}
