using Concertable.Concert.Application.Responses;
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

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is ISimpleAccept w
            ? await w.AcceptAsync(applicationId)
            : throw new BadRequestException("This contract requires a payment method at accept");
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is IPaidAccept w
            ? await w.AcceptAsync(applicationId, paymentMethodId)
            : throw new BadRequestException("This contract does not accept a payment method at accept");
    }
}
