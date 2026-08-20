using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Application.Interfaces;

/// <summary>
/// Pure, deal-type-keyed calculation of a settlement's final gross (VAT-inclusive) consideration — the
/// amount owed to the payee before platform commission. A strategy owns only the arithmetic; loading
/// eligible takings and applying commission are separate concerns. <see cref="RequiresEligibleTakings"/>
/// declares whether the deal type's formula needs the eligible takings, so the caller can avoid loading
/// them for fixed-fee deals whose gross derives entirely from immutable agreed terms.
/// </summary>
internal interface ISettlementGrossCalculator
{
    bool RequiresEligibleTakings { get; }

    Money CalculateGross(IDeal deal, decimal? eligibleTakings);
}
