using Concertable.Authorization.Contracts;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;
public class VenueHireConcertWorkflowCompleteTests
{
    private readonly Mock<IUpfrontConcertService> upfrontConcertService;
    private readonly Mock<IManagerPaymentModule> managerPaymentModule;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly VenueHireConcertWorkflow sut;

    private readonly Guid userId = Guid.NewGuid();

    public VenueHireConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IUpfrontConcertService>();
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
    public async Task FinishAsync_ShouldDelegateToUpfrontConcertService()
    {
        await sut.FinishAsync(10);

        upfrontConcertService.Verify(s => s.FinishedAsync(10), Times.Once);
    }

    [Fact]
    public async Task ApplyAsync_ShouldThrow_WhenVerifyFails()
    {
        // Arrange
        managerPaymentModule
            .Setup(m => m.VerifyAndVoidAsync(userId, "pm_bad", default))
            .ThrowsAsync(new BadRequestException("Your card was declined."));

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.ApplyAsync(1, 1, "pm_bad"));
    }
}
