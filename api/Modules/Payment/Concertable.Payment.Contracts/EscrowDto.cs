namespace Concertable.Payment.Contracts;

public record EscrowDto(
    int Id,
    int BookingId,
    Guid FromUserId,
    Guid ToUserId,
    decimal Amount,
    EscrowStatus Status,
    string ChargeId,
    string? TransferId,
    string? RefundId,
    DateTime ReleaseAt,
    DateTime? ReleasedAt,
    DateTime? RefundedAt);
