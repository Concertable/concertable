using Concertable.Payment.Contracts.Events;

namespace Concertable.Concert.Infrastructure.Events;

internal class PaymentSucceededEventHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly IPaymentSucceededProcessorFactory processorFactory;

    public PaymentSucceededEventHandler(IPaymentSucceededProcessorFactory processorFactory)
    {
        this.processorFactory = processorFactory;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var type = @event.Metadata.GetValueOrDefault("type", string.Empty);
        var processor = processorFactory.Create(type);
        await processor.HandleAsync(@event, ct);
    }
}
