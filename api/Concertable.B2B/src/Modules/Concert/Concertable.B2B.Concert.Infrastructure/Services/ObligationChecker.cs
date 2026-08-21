using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Domain.Lifecycle;
using Concertable.B2B.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.B2B.Concert.Infrastructure.Services;

internal sealed class ObligationChecker : IObligationChecker
{
    // Application states with no in-flight financial obligation — erasing a subject while an application sits in
    // any of these breaks no settlement. Every other (mid-settlement) state is a blocking obligation, so a
    // future lifecycle state defaults to "blocking" until it is deliberately classified here.
    private static readonly LifecycleState[] SettledStates =
    [
        LifecycleState.Applied,
        LifecycleState.Rejected,
        LifecycleState.Withdrawn,
        LifecycleState.Complete,
        LifecycleState.Cancelled,
    ];

    private readonly IConcertReadDbContext context;
    private readonly TimeProvider timeProvider;

    public ObligationChecker(IConcertReadDbContext context, TimeProvider timeProvider)
    {
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public async Task<bool> HasLiveAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default)
    {
        if (tenantIds.Count == 0)
            return false;

        var hasUnsettledApplication = await context.Applications
            .Where(a => tenantIds.Contains(a.VenueTenantId) || tenantIds.Contains(a.ArtistTenantId))
            .AnyAsync(a => !SettledStates.Contains(a.State), ct);
        if (hasUnsettledApplication)
            return true;

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await context.SelfBillingAgreements
            .AnyAsync(s => tenantIds.Contains(s.TenantId) && s.ExpiresAtUtc > nowUtc, ct);
    }
}
