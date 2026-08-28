using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class DoorSplitSettlementGrossCalculator : RevenueShareSettlementGrossCalculator
{
    public override Money CalculateGross(DealDto deal, Money? eligibleTakings = null) =>
        RevenueShare(eligibleTakings, ((DoorSplitDealDto)deal).ArtistDoorPercent);
}
