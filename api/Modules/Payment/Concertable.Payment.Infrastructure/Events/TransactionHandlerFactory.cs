using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Events;

internal class TransactionHandlerFactory : ITransactionHandlerFactory
{
    private readonly IServiceProvider serviceProvider;

    public TransactionHandlerFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ITransactionHandler Create(string type)
        => serviceProvider.GetRequiredKeyedService<ITransactionHandler>(type);
}
