using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Completion;

internal class CompletionDispatcher : ICompletionDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;
    private readonly ILogger<CompletionDispatcher> logger;

    public CompletionDispatcher(
        IContractLoader contractLoader,
        IConcertWorkflowFactory workflowFactory,
        ILogger<CompletionDispatcher> logger)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
        this.logger = logger;
    }

    public async Task<Result> FinishAsync(int concertId)
    {
        try
        {
            var contract = await contractLoader.LoadByConcertIdAsync(concertId);
            await workflowFactory.Create(contract.ContractType).FinishAsync(concertId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to finish concert {ConcertId}", concertId);
            return Result.Fail(ex.Message);
        }
    }
}
