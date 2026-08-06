using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Deal.Contracts;
using Concertable.B2B.Tenant.Contracts;

namespace Concertable.B2B.Concert.Infrastructure.Services.Settlement;

internal sealed class RevenueShareSettlementAmount : ISettlementAmountResolver
{
    private readonly IConcertRepository concertRepository;
    private readonly IArtistShareCalculator artistShareCalculator;
    private readonly ITenantModule tenantModule;

    public RevenueShareSettlementAmount(
        IConcertRepository concertRepository,
        IArtistShareCalculator artistShareCalculator,
        ITenantModule tenantModule)
    {
        this.concertRepository = concertRepository;
        this.artistShareCalculator = artistShareCalculator;
        this.tenantModule = tenantModule;
    }

    public async Task<decimal> ResolveGrossAsync(int concertId, IDeal deal, CancellationToken ct = default)
    {
        var settlement = await concertRepository.GetRevenueSettlementByConcertIdAsync(concertId, ct)
            ?? throw new InvalidOperationException($"Concert {concertId} reached settlement but no concert was found.");
        var totalRevenue = settlement.TotalRevenue
            ?? throw new InvalidOperationException(
                $"Concert {concertId} reached settlement with no declared door revenue — the completion gate should make this unreachable.");
        var prsRate = await tenantModule.GetPrsPassThroughRateAsync(settlement.VenueTenantId, ct);
        var prs = Math.Round(totalRevenue * prsRate, 2, MidpointRounding.AwayFromZero);
        return artistShareCalculator.Calculate(deal, totalRevenue - prs);
    }
}
