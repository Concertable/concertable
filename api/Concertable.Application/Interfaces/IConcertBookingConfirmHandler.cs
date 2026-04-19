using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IConcertBookingConfirmHandler
{
    Task HandleAsync(ConcertBookingEntity bookingConcert, ConcertEntity concert);
}
