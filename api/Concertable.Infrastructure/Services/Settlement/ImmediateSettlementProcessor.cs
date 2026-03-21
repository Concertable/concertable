using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class ImmediateSettlementProcessor : IImmediateSettlementProcessor
{
    private readonly IContractStrategyResolver<IPayImmediately> resolver;

    public ImmediateSettlementProcessor(IContractStrategyResolver<IPayImmediately> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int concertId)
    {
        var strategy = await resolver.ResolveForConcertAsync(concertId);
        await strategy.SettleAsync(concertId);
    }
}
