using Concertable.Application.Interfaces;
using Concertable.Concert.Infrastructure.Services.Settlement;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Settlement;

public class SettlementDispatcherTests
{
    private readonly Mock<IContractStrategyResolver<IConcertWorkflowStrategy>> resolver;
    private readonly SettlementDispatcher sut;

    public SettlementDispatcherTests()
    {
        resolver = new Mock<IContractStrategyResolver<IConcertWorkflowStrategy>>();
        sut = new SettlementDispatcher(resolver.Object);
    }

    [Fact]
    public async Task SettleAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>();
        resolver.Setup(r => r.ResolveForBookingAsync(1)).ReturnsAsync(strategy.Object);

        await sut.SettleAsync(1);

        strategy.Verify(s => s.SettleAsync(1), Times.Once);
    }
}
