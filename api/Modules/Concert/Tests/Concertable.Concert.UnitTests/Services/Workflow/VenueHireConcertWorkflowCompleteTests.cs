using Concertable.Authorization.Contracts;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;
public class VenueHireConcertWorkflowCompleteTests
{
    private readonly Mock<IImmediateConcertService> upfrontConcertService;
    private readonly Mock<IManagerPaymentModule> managerPaymentModule;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly VenueHireConcertWorkflow sut;

    private readonly Guid userId = Guid.NewGuid();

    public VenueHireConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IImmediateConcertService>();
        managerPaymentModule = new Mock<IManagerPaymentModule>();
        currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Id).Returns(userId);

        sut = new VenueHireConcertWorkflow(
            upfrontConcertService.Object,
            new Mock<IPayerLookup>().Object,
            new Mock<IContractLoader>().Object,
            new Mock<IApplicationRepository>().Object,
            managerPaymentModule.Object,
            currentUser.Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldDelegateToImmediateConcertService()
    {
        await sut.FinishAsync(10);

        upfrontConcertService.Verify(s => s.FinishAsync(10), Times.Once);
    }

}
