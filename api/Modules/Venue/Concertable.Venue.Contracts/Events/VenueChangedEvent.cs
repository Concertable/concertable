using Concertable.Shared;

namespace Concertable.Venue.Contracts.Events;

public record VenueChangedEvent(
    int VenueId,
    Guid UserId,
    string Name,
    string About,
    string? County,
    string? Town,
    double? Latitude,
    double? Longitude) : IIntegrationEvent;
