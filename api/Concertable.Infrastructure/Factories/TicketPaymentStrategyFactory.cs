using Concertable.Concert.Application.Interfaces;
using Concertable.Contract.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

internal interface ITicketPaymentStrategyFactory
{
    ITicketPaymentStrategy Create(ContractType type);
}

internal sealed class TicketPaymentStrategyFactory(IServiceProvider sp) : ITicketPaymentStrategyFactory
{
    public ITicketPaymentStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<ITicketPaymentStrategy>(type);
}
