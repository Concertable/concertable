using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

/// <summary>
/// Shared base for the two revenue-share gross formulae. The artist percentage is applied to the
/// eligible takings in minor units and rounded once, half-up, to the nearest minor unit — so a
/// guarantee and a revenue share are never rounded separately.
/// </summary>
internal abstract class RevenueShareSettlementGrossCalculator : ISettlementGrossCalculator
{
    public abstract Money CalculateGross(DealDto deal, Money eligibleTakings);

    protected static Money RevenueShare(Money eligibleTakings, decimal artistDoorPercent)
    {
        var shareMinor = decimal.Round(
            eligibleTakings.ToMinorUnits() * artistDoorPercent / 100m,
            0,
            MidpointRounding.AwayFromZero);
        return Money.FromMinorUnits((long)shareMinor, eligibleTakings.Currency);
    }
}
