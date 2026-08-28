using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Application.Interfaces;

/// <summary>
/// Pure, deal-type-keyed calculation of the final settlement gross owed to the payee, before any platform
/// commission: FlatFee/VenueHire return the agreed fixed term and ignore <paramref name="eligibleTakings"/>;
/// DoorSplit returns the artist percentage of eligible takings; Guarantee Plus (<see cref="DealType.Versus"/>)
/// returns the guarantee plus that percentage. The revenue-share term rounds once, half-up, to the nearest
/// minor unit. No I/O and no Payment concern — <see cref="SettlementAmountResolver"/> owns loading the takings.
/// </summary>
internal interface ISettlementGrossCalculator
{
    Money CalculateGross(DealDto deal, Money? eligibleTakings = null);
}
