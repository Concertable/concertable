using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId, Guid payerUserId, Guid payeeUserId, decimal amount);
}
