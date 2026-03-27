using Application.Interfaces;
using Application.Interfaces.Concert;
using Infrastructure.Services.Complete;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Complete;

public class CompleteProcessorTests
{
    private readonly Mock<IContractStrategyResolver<IApplicationStrategy>> resolver;
    private readonly CompleteProcessor sut;

    public CompleteProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<IApplicationStrategy>>();
        sut = new CompleteProcessor(resolver.Object);
    }

    [Fact]
    public async Task CompleteAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IApplicationStrategy>();
        resolver.Setup(r => r.ResolveForConcertAsync(1)).ReturnsAsync(strategy.Object);

        await sut.CompleteAsync(1);

        strategy.Verify(s => s.CompleteAsync(1), Times.Once);
    }
}
