using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Verify;

internal class VerifyDispatcher : IVerifyDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;
    private readonly ILogger<VerifyDispatcher> logger;

    public VerifyDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory, ILogger<VerifyDispatcher> logger)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
        this.logger = logger;
    }

    public async Task VerifyAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        if (workflow is not IVerifiable verifiable)
            throw new BadRequestException("This contract type does not support payment verification");

        logger.LogDebug("Dispatching verification for application {ApplicationId} ({ContractType})", applicationId, contract.ContractType);
        await verifiable.VerifyAsync(applicationId);
    }
}
