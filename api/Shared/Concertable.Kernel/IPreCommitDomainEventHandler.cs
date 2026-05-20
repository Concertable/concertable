namespace Concertable.Shared;

public interface IPreCommitDomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent { }
