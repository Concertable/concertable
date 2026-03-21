using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class LaterSettlementProcessor : ILaterSettlementProcessor
{
    private readonly IContractStrategyResolver<IPayLater> resolver;

    public LaterSettlementProcessor(IContractStrategyResolver<IPayLater> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.SettleAsync(concertId);
    }
}
