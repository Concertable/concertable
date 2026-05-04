using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IManagerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        CancellationToken ct = default);

    Task<Result<EscrowResponse>> HoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        DateTime releaseAt,
        CancellationToken ct = default);

    Task<Result<TransferResponse>> ReleaseAsync(
        int escrowId,
        CancellationToken ct = default);

    Task<Result<RefundResponse>> RefundAsync(
        int escrowId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<EscrowDto?> GetEscrowByBookingIdAsync(
        int bookingId,
        CancellationToken ct = default);

    Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task<CheckoutSession> CreateSetupSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task VerifyAndVoidAsync(
        Guid payerId,
        string paymentMethodId,
        CancellationToken ct = default);
}
