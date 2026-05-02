using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IUpfrontConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string paymentMethodId);
    Task SettleAsync(int bookingId);
    Task FinishedAsync(int concertId);
}
