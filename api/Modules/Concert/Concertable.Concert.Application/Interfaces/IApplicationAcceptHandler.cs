namespace Concertable.Concert.Application.Interfaces;

internal interface IApplicationAcceptHandler
{
    Task HandleAsync(int applicationId, ConcertBookingEntity bookingConcert);
}
