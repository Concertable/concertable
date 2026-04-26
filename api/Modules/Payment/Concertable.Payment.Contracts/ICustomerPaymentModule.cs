using FluentResults;

namespace Concertable.Payment.Contracts;

public interface ICustomerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string? paymentMethodId,
        CancellationToken ct = default);
}
