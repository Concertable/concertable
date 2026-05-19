using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain.Events;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure.Events;

internal class ConcertChangedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<ConcertChangedDomainEvent>
{
    public Task HandleAsync(ConcertChangedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new ConcertChangedEvent(e.ConcertId, e.TotalTickets, e.Price, e.Period, e.DatePosted), ct);
}
