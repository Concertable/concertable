namespace Concertable.Concert.Domain.Events;

public record StageAdvancedDomainEvent(int LifecycleId, ConcertStage To) : IDomainEvent;
