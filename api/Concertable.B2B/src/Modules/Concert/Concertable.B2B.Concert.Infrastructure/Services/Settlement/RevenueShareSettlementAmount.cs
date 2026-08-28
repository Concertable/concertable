using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Deal.Contracts;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

/// <summary>
/// The impure settlement-gross resolver for the two revenue-share deal types (DoorSplit, Guarantee Plus):
/// loads the eligible takings for the concert, then delegates the deal-specific formula to the pure
/// <see cref="ISettlementGrossCalculator"/>.
/// </summary>
internal sealed class RevenueShareSettlementAmount : ISettlementAmountResolver
{
    private readonly IConcertRepository concertRepository;
    private readonly ISettlementGrossCalculator grossCalculator;

    public RevenueShareSettlementAmount(
        IConcertRepository concertRepository,
        ISettlementGrossCalculator grossCalculator)
    {
        this.concertRepository = concertRepository;
        this.grossCalculator = grossCalculator;
    }

    public async Task<Money> ResolveGrossAsync(int concertId, DealDto deal, CancellationToken ct = default)
    {
        var totalRevenue = await this.concertRepository.GetTotalRevenueByConcertIdAsync(concertId)
            ?? throw new DomainException(
                $"Concert {concertId} reached settlement with no declared door revenue — the completion gate should make this unreachable.");
        return this.grossCalculator.CalculateGross(deal, Money.Gbp(totalRevenue));
    }
}
