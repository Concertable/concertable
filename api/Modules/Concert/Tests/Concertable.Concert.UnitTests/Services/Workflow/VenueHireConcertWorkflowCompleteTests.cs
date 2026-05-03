using Concertable.Authorization.Contracts;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Payment.Contracts;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;
public class VenueHireConcertWorkflowCompleteTests
{
    private readonly Mock<IUpfrontConcertService> upfrontConcertService;
    private readonly VenueHireConcertWorkflow sut;

    public VenueHireConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IUpfrontConcertService>();
        sut = new VenueHireConcertWorkflow(
            upfrontConcertService.Object,
            new Mock<IPayerLookup>().Object,
            new Mock<IContractLoader>().Object,
            new Mock<IApplicationRepository>().Object,
            new Mock<IManagerPaymentModule>().Object,
            new Mock<ICurrentUser>().Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldDelegateToUpfrontConcertService()
    {
        await sut.FinishAsync(10);

        upfrontConcertService.Verify(s => s.FinishedAsync(10), Times.Once);
    }
}
