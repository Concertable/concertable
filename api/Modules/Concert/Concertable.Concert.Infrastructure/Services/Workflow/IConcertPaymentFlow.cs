using Concertable.Payment.Contracts;
using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal interface IConcertPaymentFlow
{
    Task<string> ResolvePaymentMethodAsync(Guid payerId, string? paymentMethodId);

    Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);

    Task<CheckoutSession> CreateSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default);
}
