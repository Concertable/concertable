using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Infrastructure.Events;

public class InProcessIntegrationEventBus(IServiceProvider serviceProvider) : IIntegrationEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(@event, ct);
    }
}
