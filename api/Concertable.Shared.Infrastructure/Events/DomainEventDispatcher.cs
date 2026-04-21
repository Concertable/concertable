using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Shared.Infrastructure.Events;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var @event in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
            var handlers = serviceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
                await ((dynamic)handler!).HandleAsync((dynamic)@event, ct);
        }
    }
}
