using Concertable.Venue.Contracts.Events;
using Concertable.Venue.Domain.Events;

namespace Concertable.Venue.Infrastructure.Events;

internal class VenueChangedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<VenueChangedDomainEvent>
{
    public Task HandleAsync(VenueChangedDomainEvent e, CancellationToken ct = default)
    {
        var venue = e.Venue;
        return bus.PublishAsync(new VenueChangedEvent(
            venue.Id,
            venue.UserId,
            venue.Name,
            venue.About,
            venue.Address?.County,
            venue.Address?.Town,
            venue.Location?.Y,
            venue.Location?.X), ct);
    }
}
