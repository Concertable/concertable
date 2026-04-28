namespace Concertable.Concert.Application.Interfaces;

internal interface IBookingService
{
    Task<BookingEntity> CreateAsync(int applicationId);
    Task<BookingEntity> CreateAsync(int applicationId, string? paymentMethodId);
    Task<BookingEntity> MarkAwaitingPaymentByConcertIdAsync(int concertId);
    Task CompleteByConcertIdAsync(int concertId);
    Task CompleteAsync(int bookingId);
}
