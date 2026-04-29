using Concertable.Concert.Infrastructure.Services.Workflow;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;
public class FlatFeeConcertWorkflowCompleteTests
{
    private readonly Mock<IUpfrontConcertService> upfrontConcertService;
    private readonly FlatFeeConcertWorkflow sut;

    public FlatFeeConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IUpfrontConcertService>();
        sut = new FlatFeeConcertWorkflow(
            upfrontConcertService.Object,
            new Mock<IPayerLookup>().Object,
            new Mock<IContractLoader>().Object,
            new Mock<IConcertPaymentFlow>().Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldDelegateToUpfrontConcertService()
    {
        await sut.FinishAsync(10);

        upfrontConcertService.Verify(s => s.FinishedAsync(10), Times.Once);
    }
}
