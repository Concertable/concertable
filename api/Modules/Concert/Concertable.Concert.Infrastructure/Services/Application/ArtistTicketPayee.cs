using Concertable.Contract.Contracts;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal sealed class ArtistTicketPayee : ITicketPayee
{
    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        concert.Booking.Application.Artist.UserId;
}
