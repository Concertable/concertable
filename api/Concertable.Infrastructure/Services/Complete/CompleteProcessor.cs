using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Complete;

public class CompleteProcessor : ICompleteProcessor
{
    private readonly IContractStrategyResolver<IApplicationStrategy> resolver;

    public CompleteProcessor(IContractStrategyResolver<IApplicationStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task CompleteAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.CompleteAsync(concertId);
    }
}
