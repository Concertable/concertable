namespace Concertable.Concert.Infrastructure.Services.Settlement;

internal class SettlementDispatcher : ISettlementDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public SettlementDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task SettleAsync(int bookingId)
    {
        var contract = await contractLoader.LoadByBookingIdAsync(bookingId);
        await workflowFactory.Create(contract.ContractType).SettleAsync(bookingId);
    }
}
