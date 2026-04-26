using Concertable.Shared;

namespace Concertable.Identity.Contracts.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    Role Role) : IIntegrationEvent;

