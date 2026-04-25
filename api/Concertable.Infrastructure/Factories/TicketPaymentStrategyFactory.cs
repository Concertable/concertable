using Concertable.Payment.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

internal sealed class TicketPaymentStrategyFactory(IServiceProvider sp) : ITicketPaymentStrategyFactory
{
    public ITicketPaymentStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<ITicketPaymentStrategy>(type);
}
