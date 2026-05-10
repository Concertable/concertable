namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertService
{
    Task RegisterPaymentAsync(int applicationId, Guid payerId, string paymentMethodId);
    Task SettleAsync(int bookingId);
    Task FinishAsync(int concertId, Guid payerId, Guid payeeId, decimal amount);
}
