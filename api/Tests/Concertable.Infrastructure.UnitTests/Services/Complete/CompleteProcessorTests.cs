using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Services.Complete;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Complete;

public class CompleteProcessorTests
{
    private readonly Mock<IContractStrategyResolver<IConcertWorkflowStrategy>> resolver;
    private readonly CompleteProcessor sut;

    public CompleteProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<IConcertWorkflowStrategy>>();
        sut = new CompleteProcessor(resolver.Object);
    }

    [Fact]
    public async Task CompleteAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>();
        resolver.Setup(r => r.ResolveForConcertAsync(1)).ReturnsAsync(strategy.Object);

        await sut.CompleteAsync(1);

        strategy.Verify(s => s.CompleteAsync(1), Times.Once);
    }
}
