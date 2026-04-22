using Concertable.Application.Interfaces;
using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Infrastructure.Services.Complete;

internal class FinishedDispatcher : IFinishedDispatcher
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public FinishedDispatcher(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task<Result<IFinishOutcome>> FinishedAsync(int concertId)
    {
        try
        {
            var strategy = await resolver.ResolveForConcertAsync(concertId);
            var outcome = await strategy.FinishedAsync(concertId);
            return Result.Ok(outcome);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
