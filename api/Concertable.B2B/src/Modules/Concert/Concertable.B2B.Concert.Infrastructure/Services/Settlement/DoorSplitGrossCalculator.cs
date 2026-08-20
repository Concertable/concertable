using Concertable.B2B.Deal.Contracts;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class DoorSplitGrossCalculator : RevenueShareGrossCalculator
{
    protected override decimal GrossPounds(IDeal deal, decimal eligibleTakings)
    {
        var doorSplit = (DoorSplitDeal)deal;
        return eligibleTakings * (doorSplit.ArtistDoorPercent / 100m);
    }
}
