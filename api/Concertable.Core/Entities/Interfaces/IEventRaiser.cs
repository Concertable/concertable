using Concertable.Core.Events;

namespace Concertable.Core.Entities.Interfaces;

public interface IEventRaiser
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
