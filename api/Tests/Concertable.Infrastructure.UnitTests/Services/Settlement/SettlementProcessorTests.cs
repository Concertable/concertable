using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Services.Settlement;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Settlement;

public class SettlementProcessorTests
{
    private readonly Mock<IContractStrategyResolver<IApplicationStrategy>> resolver;
    private readonly SettlementProcessor sut;

    public SettlementProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<IApplicationStrategy>>();
        sut = new SettlementProcessor(resolver.Object);
    }

    [Fact]
    public async Task SettleAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IApplicationStrategy>();
        resolver.Setup(r => r.ResolveForApplicationAsync(1)).ReturnsAsync(strategy.Object);

        await sut.SettleAsync(1);

        strategy.Verify(s => s.SettleAsync(1), Times.Once);
    }
}
