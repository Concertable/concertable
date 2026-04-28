namespace Concertable.Concert.Infrastructure.Services.Settlement;

internal class SettlementDispatcher : ISettlementDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public SettlementDispatcher(IContractLoader contractLoader, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLoader = contractLoader;
        this.strategyFactory = strategyFactory;
    }

    public async Task SettleAsync(int bookingId)
    {
        var contract = await contractLoader.LoadByBookingIdAsync(bookingId);
        await strategyFactory.Create(contract.ContractType).SettleAsync(bookingId);
    }
}
