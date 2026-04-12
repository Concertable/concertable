using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Services.Accept;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Accept;

public class AcceptProcessorTests
{
    private readonly Mock<IContractStrategyResolver<IConcertWorkflowStrategy>> resolver;
    private readonly AcceptProcessor sut;

    public AcceptProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<IConcertWorkflowStrategy>>();
        sut = new AcceptProcessor(resolver.Object);
    }

    [Fact]
    public async Task AcceptAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>();
        resolver.Setup(r => r.ResolveForApplicationAsync(1)).ReturnsAsync(strategy.Object);

        await sut.AcceptAsync(1);

        strategy.Verify(s => s.AcceptAsync(1), Times.Once);
    }
}
