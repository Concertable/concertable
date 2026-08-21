using Reunion;

namespace Concertable.B2B.Concert.Contracts;

public interface IConcertModule
{
    Task<Option<VenueDashboardCounts>> GetVenueDashboardCountsAsync(
        Guid venueTenantId,
        CancellationToken ct = default);
    Task<Option<ArtistDashboardCounts>> GetArtistDashboardCountsAsync(
        Guid artistTenantId,
        CancellationToken ct = default);

    /// <summary>GDPR erasure gate: whether any of the subject's tenants has a live financial obligation — an
    /// unsettled application or a current self-billing agreement — so erasure defers rather than corrupting
    /// settlement. Fail-closed and answered tenant-less by explicit ids.</summary>
    Task<bool> HasLiveObligationsAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default);

    /// <summary>The subject's portable Concert records fragment (GDPR arts. 15/20): the RETAINED invoices,
    /// contracts and self-billing agreements their tenants are party to — read-only, never mutated by erasure.</summary>
    Task<ConcertRecordsExport> ExportAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default);
}
