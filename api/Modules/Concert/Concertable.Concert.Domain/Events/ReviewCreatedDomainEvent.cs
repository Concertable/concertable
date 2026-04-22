using Concertable.Shared;

namespace Concertable.Concert.Domain.Events;

public record ReviewCreatedDomainEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars) : IDomainEvent;
