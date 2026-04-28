using Concertable.Shared;

namespace Concertable.User.Contracts.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    Role Role) : IIntegrationEvent;

