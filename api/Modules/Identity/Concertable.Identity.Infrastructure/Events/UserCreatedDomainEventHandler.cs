using Concertable.Identity.Contracts.Events;
using Concertable.Identity.Domain.Events;
using Concertable.Shared;

namespace Concertable.Identity.Infrastructure.Events;

internal class UserCreatedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<UserCreatedDomainEvent>
{
    public Task HandleAsync(UserCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new UserRegisteredEvent(e.User.Id, e.User.Email, e.User.Role), ct);
}
