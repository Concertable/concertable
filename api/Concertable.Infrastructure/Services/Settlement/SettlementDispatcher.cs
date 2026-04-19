using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Settlement;

public class SettlementDispatcher : ISettlementDispatcher
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public SettlementDispatcher(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int applicationId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        await strategy.SettleAsync(applicationId);
    }
}
