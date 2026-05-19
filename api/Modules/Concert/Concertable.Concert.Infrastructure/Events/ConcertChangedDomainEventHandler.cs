using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain.Events;

namespace Concertable.Concert.Infrastructure.Events;

internal class ConcertChangedDomainEventHandler(
    IConcertRepository concertRepository,
    IIntegrationEventBus bus)
    : IDomainEventHandler<ConcertChangedDomainEvent>
{
    public async Task HandleAsync(ConcertChangedDomainEvent e, CancellationToken ct = default)
    {
        var concert = await concertRepository.GetFullByIdAsync(e.ConcertId)
            ?? throw new InvalidOperationException(
                $"Concert {e.ConcertId} not found when publishing ConcertChangedEvent");

        var artist = concert.Booking.Application.Artist;
        var venue = concert.Booking.Application.Opportunity.Venue;
        var payeeUserId = concert.ContractType == ContractType.VenueHire
            ? artist.UserId
            : venue.UserId;

        await bus.PublishAsync(new ConcertChangedEvent(
            concert.Id,
            concert.Name,
            e.TotalTickets,
            e.Price,
            e.Period,
            e.DatePosted,
            artist.Id,
            artist.Name,
            venue.Id,
            venue.Name,
            payeeUserId,
            concert.ContractType.ToString()), ct);
    }
}
