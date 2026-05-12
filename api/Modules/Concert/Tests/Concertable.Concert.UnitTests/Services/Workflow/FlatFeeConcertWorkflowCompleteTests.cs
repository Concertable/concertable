using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Payment.Contracts;
using FluentResults;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;
public class FlatFeeConcertWorkflowCompleteTests
{
    private readonly Mock<IBookingService> bookingService;
    private readonly Mock<IEscrowModule> escrowModule;
    private readonly FlatFeeConcertWorkflow sut;

    public FlatFeeConcertWorkflowCompleteTests()
    {
        bookingService = new Mock<IBookingService>();
        escrowModule = new Mock<IEscrowModule>();
        sut = new FlatFeeConcertWorkflow(
            new Mock<IApplicationValidator>().Object,
            bookingService.Object,
            escrowModule.Object,
            new Mock<IConcertDraftService>().Object,
            new Mock<IPayerLookup>().Object,
            new Mock<IContractLoader>().Object,
            new Mock<IManagerPaymentModule>().Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<FlatFeeConcertWorkflow>>().Object);
    }

    [Fact]
    public async Task FinishAsync_CompletesBookingAndReleasesEscrow()
    {
        // Arrange
        var booking = StandardBooking.Create(1);
        bookingService.Setup(s => s.CompleteByConcertIdAsync(10)).ReturnsAsync(booking);
        escrowModule.Setup(e => e.ReleaseByBookingIdAsync(It.IsAny<int>())).ReturnsAsync(Result.Ok());

        // Act
        await sut.FinishAsync(10);

        // Assert
        bookingService.Verify(s => s.CompleteByConcertIdAsync(10), Times.Once);
        escrowModule.Verify(e => e.ReleaseByBookingIdAsync(It.IsAny<int>()), Times.Once);
    }
}
