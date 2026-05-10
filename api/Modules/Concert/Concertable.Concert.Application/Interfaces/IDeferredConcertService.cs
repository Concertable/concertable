namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertService
{
    Task InitiateAsync(int applicationId, Guid payerId, string paymentMethodId);
    Task SettleAsync(int bookingId);
    Task FinishedAsync(int concertId, Guid payerId, Guid payeeId, decimal amount);
}
