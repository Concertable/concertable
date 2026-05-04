using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IEscrowModule
{
    Task<Result<EscrowResponse>> HoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        CancellationToken ct = default);

    Task<Result<TransferResponse>> ReleaseAsync(
        int escrowId,
        CancellationToken ct = default);

    Task<Result<TransferResponse?>> ReleaseByBookingIdAsync(
        int bookingId,
        CancellationToken ct = default);

    Task<Result<RefundResponse>> RefundAsync(
        int escrowId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<EscrowDto?> GetByBookingIdAsync(
        int bookingId,
        CancellationToken ct = default);
}
