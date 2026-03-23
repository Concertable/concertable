using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Settlement;

public class SettlementProcessor : ISettlementProcessor
{
    private readonly IContractStrategyResolver<IApplicationStrategy> resolver;

    public SettlementProcessor(IContractStrategyResolver<IApplicationStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int applicationId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        await strategy.SettleAsync(applicationId);
    }
}
