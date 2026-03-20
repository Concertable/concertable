using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Complete;

public class CompleteProcessor : ICompleteProcessor
{
    private readonly IContractStrategyResolver<ICompleteStrategy> resolver;

    public CompleteProcessor(IContractStrategyResolver<ICompleteStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task CompleteAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.CompleteAsync(concertId);
    }
}
