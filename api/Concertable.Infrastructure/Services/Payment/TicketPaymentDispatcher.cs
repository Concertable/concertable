using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

internal class TicketPaymentDispatcher : ITicketPaymentDispatcher
{
    private readonly IContractStrategyResolver<ITicketPaymentStrategy> resolver;

    public TicketPaymentDispatcher(IContractStrategyResolver<ITicketPaymentStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        return await strategy.PayAsync(concertId, quantity, paymentMethodId, price);
    }
}
