using Concertable.Application.Interfaces;

namespace Concertable.Infrastructure.Services.Settlement;

internal class SettlementDispatcher : ISettlementDispatcher
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public SettlementDispatcher(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task SettleAsync(int bookingId)
    {
        var strategy = await resolver.ResolveForBookingAsync(bookingId);
        await strategy.SettleAsync(bookingId);
    }
}
