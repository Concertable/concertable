namespace Concertable.Application.Interfaces.Concert;

public interface IConcertWorkflowStrategy : IContractStrategy
{
    Task AcceptAsync(int applicationId);
    Task SettleAsync(int applicationId);
    Task CompleteAsync(int concertId);
}
