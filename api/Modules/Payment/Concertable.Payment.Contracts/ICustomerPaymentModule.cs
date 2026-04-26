using FluentResults;

namespace Concertable.Payment.Contracts;

public interface ICustomerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid customerUserId,
        Guid payeeUserId,
        decimal amount,
        int referenceId,
        int count,
        string? paymentMethodId,
        CancellationToken ct = default);
}
