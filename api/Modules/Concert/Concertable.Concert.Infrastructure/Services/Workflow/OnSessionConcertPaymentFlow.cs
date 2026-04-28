using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class OnSessionConcertPaymentFlow : IConcertPaymentFlow
{
    private readonly IManagerPaymentModule managerPaymentModule;

    public OnSessionConcertPaymentFlow(
        [FromKeyedServices(PaymentSession.OnSession)] IManagerPaymentModule managerPaymentModule)
    {
        this.managerPaymentModule = managerPaymentModule;
    }

    public async Task<string> ResolvePaymentMethodAsync(Guid payerId, string? paymentMethodId)
    {
        return paymentMethodId
            ?? await managerPaymentModule.TryGetPaymentMethodIdAsync(payerId)
            ?? throw new BadRequestException("A payment method is required.");
    }

    public Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        IDictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        return managerPaymentModule.PayAsync(payerId, payeeId, amount, metadata, paymentMethodId, ct);
    }
}
