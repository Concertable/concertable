using Concertable.Application.Interfaces;
using Concertable.Concert.Application.Interfaces;
using Concertable.Infrastructure.Factories;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

internal class TicketPaymentDispatcher : ITicketPaymentDispatcher
{
    private readonly IContractLookup contractLookup;
    private readonly ITicketPaymentStrategyFactory strategyFactory;

    public TicketPaymentDispatcher(IContractLookup contractLookup, ITicketPaymentStrategyFactory strategyFactory)
    {
        this.contractLookup = contractLookup;
        this.strategyFactory = strategyFactory;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var contract = await contractLookup.GetByConcertIdAsync(concertId);
        return await strategyFactory.Create(contract.ContractType)
            .PayAsync(concertId, quantity, paymentMethodId, price);
    }
}
