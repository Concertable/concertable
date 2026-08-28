using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

/// <summary>
/// Guarantee Plus (the commercial formula carried by <see cref="DealType.Versus"/>): the agreed guarantee
/// plus the artist percentage of eligible takings — never "whichever is greater".
/// </summary>
internal sealed class VersusSettlementGrossCalculator : RevenueShareSettlementGrossCalculator
{
    public override Money CalculateGross(DealDto deal, Money eligibleTakings)
    {
        var versus = (VersusDealDto)deal;
        return Money.Gbp(versus.Guarantee) + RevenueShare(eligibleTakings, versus.ArtistDoorPercent);
    }
}
