using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Services.Complete;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Complete;

public class CompleteProcessorTests
{
    private readonly Mock<IContractStrategyResolver<IConcertWorkflowStrategy>> resolver;
    private readonly FinishedProcessor sut;

    public CompleteProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<IConcertWorkflowStrategy>>();
        sut = new FinishedProcessor(resolver.Object);
    }

    [Fact]
    public async Task FinishedAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>();
        resolver.Setup(r => r.ResolveForConcertAsync(1)).ReturnsAsync(strategy.Object);

        await sut.FinishedAsync(1);

        strategy.Verify(s => s.FinishedAsync(1), Times.Once);
    }
}
