using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Infrastructure.Services.Accept;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Accept;

public class AcceptDispatcherTests
{
    private readonly Mock<IContractStrategyResolver<IConcertWorkflowStrategy>> resolver;
    private readonly AcceptDispatcher sut;

    public AcceptDispatcherTests()
    {
        resolver = new Mock<IContractStrategyResolver<IConcertWorkflowStrategy>>();
        sut = new AcceptDispatcher(resolver.Object);
    }

    [Fact]
    public async Task AcceptAsync_ShouldResolveStrategyAndDelegate()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>();
        resolver.Setup(r => r.ResolveForApplicationAsync(1)).ReturnsAsync(strategy.Object);

        await sut.AcceptAsync(1);

        strategy.Verify(s => s.InitiateAsync(1, null), Times.Once);
    }
}
