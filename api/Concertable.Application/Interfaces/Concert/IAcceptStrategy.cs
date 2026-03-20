namespace Application.Interfaces.Concert;

public interface IAcceptStrategy : IContractStrategy
{
    Task AcceptAsync(int applicationId);
}
