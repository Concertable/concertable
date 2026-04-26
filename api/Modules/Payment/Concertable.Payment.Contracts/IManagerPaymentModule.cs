using FluentResults;

namespace Concertable.Payment.Contracts;

public interface IManagerPaymentModule
{
    Task<Result<PaymentResponse>> PayAsync(
        Guid payerUserId,
        Guid payeeUserId,
        decimal amount,
        int referenceId,
        string? paymentMethodId,
        CancellationToken ct = default);
}
