using Concertable.Concert.Application.Responses;
using Concertable.Payment.Contracts;

namespace Concertable.Concert.Application.Interfaces;

internal interface IUpfrontConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string paymentMethodId, PaymentSession session);
    Task SettleAsync(int bookingId);
    Task FinishedAsync(int concertId);
}
