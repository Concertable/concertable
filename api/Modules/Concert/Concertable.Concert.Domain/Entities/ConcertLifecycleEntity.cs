using Concertable.Concert.Domain.Events;

namespace Concertable.Concert.Domain;

public class ConcertLifecycleEntity : IIdEntity, IEventRaiser
{
    public int Id { get; private set; }
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public ConcertStage Stage { get; private set; } = ConcertStage.None;
    public DateTime CreatedAt { get; private set; }

    private readonly EventRaiser events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => events.DomainEvents;
    public void ClearDomainEvents() => events.Clear();

    private ConcertLifecycleEntity() { }

    public static ConcertLifecycleEntity Create(int opportunityId, int artistId) => new()
    {
        OpportunityId = opportunityId,
        ArtistId = artistId,
        CreatedAt = DateTime.UtcNow
    };

    public void AdvanceTo(ConcertStage to)
    {
        if (to <= Stage)
            throw new DomainException($"Cannot advance lifecycle from {Stage} to {to}.");

        Stage = to;
        events.Raise(new StageAdvancedDomainEvent(Id, to));
    }
}
