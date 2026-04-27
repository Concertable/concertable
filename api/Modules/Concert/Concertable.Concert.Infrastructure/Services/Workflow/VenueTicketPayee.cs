using Concertable.Contract.Contracts;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class VenueTicketPayee : ITicketPayee
{
    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        concert.Booking.Application.Opportunity.Venue.UserId;
}
