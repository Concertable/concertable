using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Complete;

public class FinishedProcessor : IFinishedProcessor
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public FinishedProcessor(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task FinishedAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.FinishedAsync(concertId);
    }
}
