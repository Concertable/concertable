using Concertable.B2B.Deal.Contracts;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class VersusGrossCalculator : RevenueShareGrossCalculator
{
    protected override decimal GrossPounds(IDeal deal, decimal eligibleTakings)
    {
        var versus = (VersusDeal)deal;
        return versus.Guarantee + (eligibleTakings * (versus.ArtistDoorPercent / 100m));
    }
}
