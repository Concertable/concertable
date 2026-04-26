using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Events;

internal class TransactionStrategyFactory : ITransactionStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public TransactionStrategyFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ITransactionStrategy Create(string type)
        => serviceProvider.GetRequiredKeyedService<ITransactionStrategy>(type);
}
