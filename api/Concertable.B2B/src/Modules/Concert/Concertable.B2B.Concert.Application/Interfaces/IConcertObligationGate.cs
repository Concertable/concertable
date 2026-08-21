namespace Concertable.B2B.Concert.Application.Interfaces;

/// <summary>The fail-closed erasure gate's Concert leg: whether any of the subject's tenants has a live
/// financial obligation — an unsettled application (escrow/settlement in flight) or a current self-billing
/// agreement — answered by explicit tenant ids over the unfiltered read stance, so it works tenant-less from
/// the admin erasure flow. Mirrors <c>ISelfBillingAgreementGate</c>: a plain <c>bool</c> the caller turns into
/// a deferral.</summary>
internal interface IConcertObligationGate
{
    Task<bool> HasLiveObligationsAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct = default);
}
