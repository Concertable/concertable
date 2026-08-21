namespace Concertable.B2B.Privacy.Application.Interfaces;

/// <summary>The fail-closed erasure gate: whether a subject has any live financial obligation that must defer
/// their erasure. Answered by fanning the subject's tenants out to the modules that can see an obligation
/// (Concert today; Payment joins in Phase 4). A <c>true</c> defers; the gate never throws to signal it.</summary>
internal interface IErasureGate
{
    Task<bool> HasLiveObligationsAsync(Guid subjectId, CancellationToken ct = default);
}
