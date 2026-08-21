using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Contracts;

namespace Concertable.B2B.Concert.Infrastructure;

internal sealed class ConcertModule : IConcertModule
{
    private readonly IConcertDashboardService dashboardService;
    private readonly IObligationChecker obligationChecker;
    private readonly IConcertRecordsExporter recordsExporter;

    public ConcertModule(
        IConcertDashboardService dashboardService,
        IObligationChecker obligationChecker,
        IConcertRecordsExporter recordsExporter)
    {
        this.dashboardService = dashboardService;
        this.obligationChecker = obligationChecker;
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
        obligationChecker.HasLiveAsync(tenantIds, ct);

    public Task<ConcertRecordsExport> ExportAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default) =>
        recordsExporter.ExportAsync(tenantIds, ct);
}
