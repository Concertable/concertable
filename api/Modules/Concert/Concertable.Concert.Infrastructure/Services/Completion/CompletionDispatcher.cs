using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Completion;

internal class CompletionDispatcher : ICompletionDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public CompletionDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<Result<IFinishOutcome>> FinishAsync(int concertId)
    {
        try
        {
            var contract = await contractLoader.LoadByConcertIdAsync(concertId);
            var outcome = await workflowFactory.Create(contract.ContractType).FinishAsync(concertId);
            return Result.Ok(outcome);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
