namespace Application.Interfaces.Concert;

public interface IContractSettlementStrategy : IContractStrategy
{
    Task SettleAsync(int concertId);
}
