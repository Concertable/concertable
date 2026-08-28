namespace Concertable.B2B.Concert.Domain.ValueObjects;

/// <summary>
/// The payer's confirmed review of a revenue-share settlement: the frozen payee gross (VAT-inclusive, minor
/// units) and when the payer confirmed it. Held as one value so a settlement is either reviewed or not —
/// there is no half-reviewed state.
/// </summary>
public sealed record SettlementReview(long GrossMinor, DateTime ReviewedAtUtc);
