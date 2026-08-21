namespace Concertable.B2B.Privacy.Infrastructure.Services;

internal sealed class ErasureGate : IErasureGate
{
    private readonly ITenantModule tenantModule;
    private readonly IConcertModule concertModule;

    public ErasureGate(ITenantModule tenantModule, IConcertModule concertModule)
    {
        this.tenantModule = tenantModule;
        this.concertModule = concertModule;
    }

    public async Task<bool> HasLiveObligationsAsync(Guid subjectId, CancellationToken ct = default)
    {
        var memberships = await tenantModule.GetMembershipsAsync(subjectId, ct);
        var tenantIds = memberships.Select(m => m.TenantId).Distinct().ToArray();
        if (tenantIds.Length == 0)
            return false;

        return await concertModule.HasLiveObligationsAsync(tenantIds, ct);
    }
}
