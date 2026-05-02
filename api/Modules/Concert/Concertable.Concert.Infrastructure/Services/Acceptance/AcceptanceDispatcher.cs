using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Infrastructure.Services.Acceptance;

internal class AcceptanceDispatcher : IAcceptanceDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public AcceptanceDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<AcceptCheckout?> CheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is IAcceptCheckout co
            ? await co.CheckoutAsync(applicationId)
            : null;
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        return await workflowFactory.Create(contract.ContractType)
            .InitiateAsync(applicationId, paymentMethodId);
    }
}
