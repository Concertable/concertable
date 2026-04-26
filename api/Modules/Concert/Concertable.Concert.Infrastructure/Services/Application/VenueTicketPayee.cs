using Concertable.Contract.Abstractions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal sealed class VenueTicketPayee : ITicketPayee
{
    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        concert.Booking.Application.Opportunity.Venue.UserId;
}
