using Concertable.Concert.Application.Responses;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal class CheckoutDispatcher : ICheckoutDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public CheckoutDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<Checkout> ApplyCheckoutAsync(int opportunityId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is IApplyCheckout w
            ? await w.CheckoutAsync(opportunityId)
            : throw new BadRequestException("This contract does not have an apply-time checkout");
    }

    public async Task<Checkout> AcceptCheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is IAcceptCheckout w
            ? await w.CheckoutAsync(applicationId)
            : throw new BadRequestException("This contract does not have an accept-time checkout");
    }
}
