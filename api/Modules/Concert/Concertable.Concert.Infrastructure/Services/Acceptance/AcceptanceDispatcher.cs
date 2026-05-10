using Concertable.Shared.Exceptions;

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

    public async Task AcceptAsync(int applicationId, string? paymentMethodId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);

        switch (workflow)
        {
            case IPaidAccept w when paymentMethodId is not null:
                await w.AcceptAsync(applicationId, paymentMethodId);
                break;

            case ISimpleAccept w:
                await w.AcceptAsync(applicationId);
                break;

            default:
                throw new BadRequestException("Accept payload does not match this contract's payment shape");
        }
    }
}
