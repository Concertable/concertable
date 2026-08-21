namespace Concertable.B2B.Concert.Application.Interfaces;

/// <summary>Checks whether a subject's tenants carry a live financial obligation — an unsettled application
/// (escrow/settlement in flight) or a current self-billing agreement — answered across all tenants over the
/// unfiltered read stance so it works tenant-less from the admin erasure flow. A boolean precondition read
/// the GDPR erasure flow turns into a deferral; it does not throw.</summary>
internal interface IObligationChecker
{
    Task<bool> HasLiveAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default);
}
