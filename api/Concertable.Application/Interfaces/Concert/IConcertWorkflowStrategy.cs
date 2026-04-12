namespace Concertable.Application.Interfaces.Concert;

public interface IConcertWorkflowStrategy : IContractStrategy
{
    Task InitiateAsync(int applicationId);
    Task SettleAsync(int applicationId);
    Task FinishedAsync(int concertId);
}
