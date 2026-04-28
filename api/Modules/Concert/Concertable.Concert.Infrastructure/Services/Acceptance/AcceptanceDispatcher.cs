using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Infrastructure.Services.Acceptance;

internal class AcceptanceDispatcher : IAcceptanceExecutor
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public AcceptanceDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        return await workflowFactory.Create(contract.ContractType)
            .CheckoutAsync(applicationId);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        return await workflowFactory.Create(contract.ContractType)
            .InitiateAsync(applicationId, paymentMethodId);
    }
}
