using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Complete;

public class CompleteProcessor : ICompleteProcessor
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public CompleteProcessor(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task CompleteAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.CompleteAsync(concertId);
    }
}
