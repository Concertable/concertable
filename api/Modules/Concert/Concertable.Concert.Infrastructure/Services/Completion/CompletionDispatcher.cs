using FluentResults;

namespace Concertable.Concert.Infrastructure.Services.Completion;

internal class CompletionDispatcher : ICompletionDispatcher
{
    private readonly IContractLookup contractLookup;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public CompletionDispatcher(IContractLookup contractLookup, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLookup = contractLookup;
        this.strategyFactory = strategyFactory;
    }

    public async Task<Result<IFinishOutcome>> FinishAsync(int concertId)
    {
        try
        {
            var contract = await contractLookup.GetByConcertIdAsync(concertId);
            var outcome = await strategyFactory.Create(contract.ContractType).FinishAsync(concertId);
            return Result.Ok(outcome);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
