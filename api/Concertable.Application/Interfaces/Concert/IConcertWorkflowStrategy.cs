namespace Concertable.Application.Interfaces.Concert;

public interface IConcertWorkflowStrategy : IContractStrategy
{
    Task InitiateAsync(int applicationId, string? paymentMethodId = null);
    Task SettleAsync(int applicationId);
    Task FinishedAsync(int concertId);
}
