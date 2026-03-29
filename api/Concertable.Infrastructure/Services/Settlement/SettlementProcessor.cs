using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Settlement;

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
