using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class OffSessionConcertPaymentFlow : IConcertPaymentFlow
{
    private readonly IManagerPaymentModule managerPaymentModule;

    public OffSessionConcertPaymentFlow(IManagerPaymentModule managerPaymentModule)
    {
        this.managerPaymentModule = managerPaymentModule;
    }

    public async Task<string> ResolvePaymentMethodAsync(Guid payerId, string? paymentMethodId)
    {
        if (!await managerPaymentModule.HasStripeCustomerAsync(payerId))
            throw new BadRequestException("Stripe customer setup is required for deferred payments.");

        return paymentMethodId
            ?? await managerPaymentModule.TryGetPaymentMethodIdAsync(payerId)
            ?? throw new BadRequestException("A payment method is required.");
    }

    public Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        return managerPaymentModule.PayAsync(payerId, payeeId, amount, metadata, paymentMethodId, PaymentSession.OffSession, ct);
    }

    public Task<CheckoutSession> CreateSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        return managerPaymentModule.CreateSavedCardSessionAsync(payerId, metadata, ct);
    }
}
