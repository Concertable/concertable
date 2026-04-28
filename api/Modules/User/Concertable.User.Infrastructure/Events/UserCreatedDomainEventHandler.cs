using Concertable.User.Contracts.Events;
using Concertable.User.Domain.Events;
using Concertable.Shared;

namespace Concertable.User.Infrastructure.Events;

internal class UserCreatedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<UserCreatedDomainEvent>
{
    public Task HandleAsync(UserCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new UserRegisteredEvent(e.User.Id, e.User.Email, e.User.Role), ct);
}
