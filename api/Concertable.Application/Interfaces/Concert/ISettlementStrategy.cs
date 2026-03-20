namespace Application.Interfaces.Concert;

public interface ISettlementStrategy : IContractStrategy
{
    Task SettleAsync(int concertId);
}
