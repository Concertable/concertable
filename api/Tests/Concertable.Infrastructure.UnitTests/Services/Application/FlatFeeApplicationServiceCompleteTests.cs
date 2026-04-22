using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class FlatFeeConcertWorkflowCompleteTests
{
    private readonly Mock<IUpfrontConcertService> upfrontConcertService;
    private readonly FlatFeeConcertWorkflow sut;

    public FlatFeeConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IUpfrontConcertService>();
        sut = new FlatFeeConcertWorkflow(
            upfrontConcertService.Object,
            new Mock<IContractRepository>().Object,
            new Mock<IManagerModule>().Object);
    }

    [Fact]
    public async Task FinishedAsync_ShouldDelegateToUpfrontConcertService()
    {
        await sut.FinishedAsync(10);

        upfrontConcertService.Verify(s => s.FinishedAsync(10), Times.Once);
    }
}
