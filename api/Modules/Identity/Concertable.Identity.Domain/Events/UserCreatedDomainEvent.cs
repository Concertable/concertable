namespace Concertable.Identity.Domain.Events;

public record UserCreatedDomainEvent(UserEntity User) : IDomainEvent;
