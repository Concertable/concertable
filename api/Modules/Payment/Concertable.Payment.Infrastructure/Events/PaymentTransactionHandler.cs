using Concertable.Payment.Contracts.Events;

namespace Concertable.Payment.Infrastructure.Events;

internal class PaymentTransactionHandler : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly ITransactionHandlerFactory handlerFactory;

    public PaymentTransactionHandler(ITransactionHandlerFactory handlerFactory)
    {
        this.handlerFactory = handlerFactory;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var type = @event.Metadata.GetValueOrDefault("type", string.Empty);
        var handler = handlerFactory.Create(type);
        await handler.HandleAsync(@event, ct);
    }
}
