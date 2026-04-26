using Concertable.Payment.Contracts.Events;

namespace Concertable.Payment.Infrastructure.Events;

internal class PaymentTransactionHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly ITransactionStrategyFactory strategyFactory;

    public PaymentTransactionHandler(ITransactionStrategyFactory strategyFactory)
    {
        this.strategyFactory = strategyFactory;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var type = @event.Metadata.GetValueOrDefault("type", string.Empty);
        var strategy = strategyFactory.Create(type);
        await strategy.HandleAsync(@event, ct);
    }
}
