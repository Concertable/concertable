using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Apply;

internal class ApplyDispatcher : IApplyDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public ApplyDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<ApplicationEntity> ApplyAsync(int opportunityId, int artistId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is ISimpleApply simple
            ? await simple.ApplyAsync(artistId, opportunityId)
            : throw new BadRequestException("This contract requires a payment method at apply");
    }

    public async Task<ApplicationEntity> ApplyAsync(int opportunityId, int artistId, string paymentMethodId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is IApplyWithPaymentMethod withPm
            ? await withPm.ApplyAsync(artistId, opportunityId, paymentMethodId)
            : throw new BadRequestException("This contract does not accept a payment method at apply");
    }
}
