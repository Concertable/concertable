using Concertable.Shared;

namespace Concertable.Concert.Contracts.Events;

public record ConcertChangedEvent(
    int ConcertId,
    int TotalTickets,
    decimal Price,
    DateRange Period,
    DateTime? DatePosted) : IIntegrationEvent;
