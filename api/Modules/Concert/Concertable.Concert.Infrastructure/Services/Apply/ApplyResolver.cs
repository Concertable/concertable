using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Apply;

internal class ApplyResolver : IApplyResolver
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public ApplyResolver(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<ISimpleApply> ResolveSimpleAsync(int opportunityId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        return workflowFactory.Create(contract.ContractType) is ISimpleApply simple
            ? simple
            : throw new BadRequestException("This contract requires a payment method at apply");
    }

    public async Task<IApplyWithPaymentMethod> ResolveWithPaymentMethodAsync(int opportunityId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        return workflowFactory.Create(contract.ContractType) is IApplyWithPaymentMethod withPm
            ? withPm
            : throw new BadRequestException("This contract does not accept a payment method at apply");
    }
}
