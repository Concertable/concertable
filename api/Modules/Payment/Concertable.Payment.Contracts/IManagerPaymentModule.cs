using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IManagerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string paymentMethodId,
        CancellationToken ct = default);

    Task<bool> HasStripeCustomerAsync(Guid userId);
    Task<string?> TryGetPaymentMethodIdAsync(Guid userId);

    Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task<CheckoutSession> CreateSavedCardSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);
}
