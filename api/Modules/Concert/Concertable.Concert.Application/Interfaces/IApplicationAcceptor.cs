namespace Concertable.Concert.Application.Interfaces;

internal interface IApplicationAcceptor
{
    Task AcceptAsync(int applicationId, BookingEntity booking);
}
