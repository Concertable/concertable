using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, string paymentMethodId);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId, Guid payerId, Guid payeeId, decimal amount);
}
