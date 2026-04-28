using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Completion;

internal class CompletionDispatcher : ICompletionDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public CompletionDispatcher(IContractLoader contractLoader, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLoader = contractLoader;
        this.strategyFactory = strategyFactory;
    }

    public async Task<Result<IFinishOutcome>> FinishAsync(int concertId)
    {
        try
        {
            var contract = await contractLoader.LoadByConcertIdAsync(concertId);
            var outcome = await strategyFactory.Create(contract.ContractType).FinishAsync(concertId);
            return Result.Ok(outcome);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
