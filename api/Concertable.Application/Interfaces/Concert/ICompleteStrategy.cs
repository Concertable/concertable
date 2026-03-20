namespace Application.Interfaces.Concert;

public interface ICompleteStrategy : IContractStrategy
{
    Task CompleteAsync(int concertId);
}
