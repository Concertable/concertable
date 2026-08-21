using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Contracts;

namespace Concertable.B2B.Concert.Infrastructure;

internal sealed class ConcertModule : IConcertModule
{
    private readonly IConcertDashboardService dashboardService;
    private readonly IConcertObligationGate obligationGate;
    private readonly IConcertRecordsExporter recordsExporter;

    public ConcertModule(
        IConcertDashboardService dashboardService,
        IConcertObligationGate obligationGate,
        IConcertRecordsExporter recordsExporter)
    {
        this.dashboardService = dashboardService;
        this.obligationGate = obligationGate;
        this.recordsExporter = recordsExporter;
    }

    public Task<Option<VenueDashboardCounts>> GetVenueDashboardCountsAsync(
        Guid venueTenantId,
        CancellationToken ct = default) =>
        dashboardService.GetVenueCountsAsync(venueTenantId, ct);

    public Task<Option<ArtistDashboardCounts>> GetArtistDashboardCountsAsync(
        Guid artistTenantId,
        CancellationToken ct = default) =>
        dashboardService.GetArtistCountsAsync(artistTenantId, ct);

    public Task<bool> HasLiveObligationsAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default) =>
        obligationGate.HasLiveObligationsAsync(tenantIds, ct);

    public Task<ConcertRecordsExport> ExportAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default) =>
        recordsExporter.ExportAsync(tenantIds, ct);
}
