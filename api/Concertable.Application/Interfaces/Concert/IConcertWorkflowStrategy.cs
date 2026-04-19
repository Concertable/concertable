using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

public interface IConcertWorkflowStrategy : IContractStrategy
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null);
    Task SettleAsync(int applicationId);
    Task<IFinishOutcome> FinishedAsync(int concertId);
}
