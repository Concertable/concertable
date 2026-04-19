using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Handlers;

public class ConcertBookingConfirmHandler : IConcertBookingConfirmHandler
{
    private readonly IConcertBookingRepository bookingRepository;

    public ConcertBookingConfirmHandler(IConcertBookingRepository bookingRepository)
    {
        this.bookingRepository = bookingRepository;
    }

    public async Task HandleAsync(ConcertBookingEntity bookingConcert, ConcertEntity concert)
    {
        bookingConcert.Confirm(concert);
        await bookingRepository.SaveChangesAsync();
    }
}
