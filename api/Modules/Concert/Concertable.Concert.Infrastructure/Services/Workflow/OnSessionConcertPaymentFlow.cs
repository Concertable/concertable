using Concertable.Payment.Contracts;
using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class OnSessionConcertPaymentFlow : IConcertPaymentFlow
{
    private readonly IManagerPaymentModule managerPaymentModule;

    public OnSessionConcertPaymentFlow(IManagerPaymentModule managerPaymentModule)
    {
        this.managerPaymentModule = managerPaymentModule;
    }

    public Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        return managerPaymentModule.PayAsync(payerId, payeeId, amount, metadata, paymentMethodId, PaymentSession.OnSession, ct);
    }

    public Task<CheckoutSession> CreateSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        return managerPaymentModule.CreatePaymentSessionAsync(payerId, metadata, ct);
    }
}
