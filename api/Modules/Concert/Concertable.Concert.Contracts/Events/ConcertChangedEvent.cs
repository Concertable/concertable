using Concertable.Messaging;
using Concertable.Shared;

namespace Concertable.Concert.Contracts.Events;

public record ConcertChangedEvent(
    int ConcertId,
    string Name,
    int TotalTickets,
    decimal Price,
    DateRange Period,
    DateTime? DatePosted,
    int ArtistId,
    string ArtistName,
    int VenueId,
    string VenueName,
    Guid PayeeUserId,
    string ContractType) : IIntegrationEvent;
