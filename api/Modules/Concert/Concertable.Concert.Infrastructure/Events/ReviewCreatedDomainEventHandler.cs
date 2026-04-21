using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain.Events;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure.Events;

internal class ReviewCreatedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<ReviewCreatedDomainEvent>
{
    public Task HandleAsync(ReviewCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new ReviewSubmittedEvent(e.ArtistId, e.VenueId, e.ConcertId, e.Stars), ct);
}
