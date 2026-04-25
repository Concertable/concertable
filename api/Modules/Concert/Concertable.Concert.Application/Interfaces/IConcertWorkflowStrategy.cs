using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertWorkflowStrategy : IContractStrategy
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishAsync(int concertId);
}
