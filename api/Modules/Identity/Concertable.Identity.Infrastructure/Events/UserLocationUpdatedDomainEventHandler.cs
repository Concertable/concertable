using Concertable.Identity.Contracts.Events;
using Concertable.Identity.Domain.Events;
using Concertable.Shared;

namespace Concertable.Identity.Infrastructure.Events;

internal class UserLocationUpdatedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<UserLocationUpdatedDomainEvent>
{
    public Task HandleAsync(UserLocationUpdatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new UserLocationUpdatedEvent(e.UserId, e.Location, e.Address), ct);
}
