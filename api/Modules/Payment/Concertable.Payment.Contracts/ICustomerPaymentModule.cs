using FluentResults;

namespace Concertable.Payment.Contracts;

public interface ICustomerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid customerUserId,
        Guid payeeUserId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string? paymentMethodId,
        CancellationToken ct = default);
}
