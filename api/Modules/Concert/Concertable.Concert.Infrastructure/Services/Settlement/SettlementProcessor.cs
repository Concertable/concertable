using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Settlement;

internal class SettlementExecutor : ISettlementExecutor
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;
    private readonly ILogger<SettlementExecutor> logger;

    public SettlementExecutor(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory, ILogger<SettlementExecutor> logger)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
        this.logger = logger;
    }

    public async Task SettleAsync(int bookingId)
    {
        var contract = await contractLoader.TryLoadByBookingIdAsync(bookingId);
        if (contract is null)
            return;

        logger.LogDebug("Dispatching settlement for booking {BookingId} ({ContractType})", bookingId, contract.ContractType);

        await workflowFactory.Create(contract.ContractType).SettleAsync(bookingId);
    }
}
