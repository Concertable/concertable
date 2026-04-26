using Concertable.Payment.Contracts.Events;

namespace Concertable.Concert.Infrastructure.Events;

internal class PaymentSucceededEventHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly IPaymentSucceededStrategyFactory strategyFactory;

    public PaymentSucceededEventHandler(IPaymentSucceededStrategyFactory strategyFactory)
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
