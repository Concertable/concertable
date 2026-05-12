using Concertable.Payment.Contracts;

namespace Concertable.Concert.Application.Interfaces;

internal interface IImmediateConcertService
{
    Task ChargeAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string paymentMethodId, PaymentSession session);
    Task SettleAsync(int bookingId);
    Task FinishAsync(int concertId);
}
