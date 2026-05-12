namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertService
{
    Task RegisterPaymentAsync(int applicationId, string paymentMethodId);
    Task VerifyAsync(int applicationId);
    Task SettleAsync(int bookingId);
    Task FinishAsync(int concertId, Guid payerId, Guid payeeId, decimal amount);
}
