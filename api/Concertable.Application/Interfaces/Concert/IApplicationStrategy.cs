namespace Application.Interfaces.Concert;

public interface IApplicationStrategy : IContractStrategy
{
    Task AcceptAsync(int applicationId);
    Task SettleAsync(int applicationId);
    Task CompleteAsync(int concertId);
}
