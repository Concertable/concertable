namespace Application.Interfaces.Concert;

public interface IContractSettlementService : IContractWorkflow
{
    Task SettleAsync(int concertId);
}
