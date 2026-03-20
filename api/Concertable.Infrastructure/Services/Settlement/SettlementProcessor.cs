using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class SettlementProcessor : ISettlementProcessor
{
    private readonly IContractStrategyResolver<IContractSettlementStrategy> resolver;

    public SettlementProcessor(IContractStrategyResolver<IContractSettlementStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.SettleAsync(concertId);
    }
}
