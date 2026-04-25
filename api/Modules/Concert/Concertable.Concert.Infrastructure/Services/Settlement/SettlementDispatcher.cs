namespace Concertable.Concert.Infrastructure.Services.Settlement;

internal class SettlementDispatcher : ISettlementDispatcher
{
    private readonly IContractLookup contractLookup;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public SettlementDispatcher(IContractLookup contractLookup, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLookup = contractLookup;
        this.strategyFactory = strategyFactory;
    }

    public async Task SettleAsync(int bookingId)
    {
        var contract = await contractLookup.GetByBookingIdAsync(bookingId);
        await strategyFactory.Create(contract.ContractType).SettleAsync(bookingId);
    }
}
