using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class VenueHireGrossCalculator : ISettlementGrossCalculator
{
    public bool RequiresEligibleTakings => false;

    public Money CalculateGross(IDeal deal, decimal? eligibleTakings) =>
        SettlementGross.InGbp(((VenueHireDeal)deal).HireFee);
}
