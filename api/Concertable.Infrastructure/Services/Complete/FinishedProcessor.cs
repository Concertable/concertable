using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using FluentResults;

namespace Concertable.Infrastructure.Services.Complete;

public class FinishedProcessor : IFinishedProcessor
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public FinishedProcessor(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task<Result> FinishedAsync(int concertId)
    {
        try
        {
            var strategy = await resolver.ResolveForConcertAsync(concertId);
            await strategy.FinishedAsync(concertId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
