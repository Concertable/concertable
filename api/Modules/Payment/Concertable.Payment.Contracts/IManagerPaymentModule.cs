using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IManagerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        PaymentSession session,
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
