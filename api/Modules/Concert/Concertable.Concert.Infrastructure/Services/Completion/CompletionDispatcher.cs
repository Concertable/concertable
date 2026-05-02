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
            return Result.Fail(ex.Message);
        }
    }
}
