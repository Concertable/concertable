using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class FlatFeeApplicationServiceCompleteTests
{
    private readonly Mock<IOpportunityApplicationRepository> applicationRepository;
    private readonly FlatFeeApplicationService sut;

    public FlatFeeApplicationServiceCompleteTests()
    {
        applicationRepository = new Mock<IOpportunityApplicationRepository>();
        sut = new FlatFeeApplicationService(
            new Mock<IOpportunityApplicationValidator>().Object,
            applicationRepository.Object,
            new Mock<IContractRepository>().Object,
            new Mock<IVenueManagerRepository>().Object,
            new Mock<IArtistManagerRepository>().Object,
            new Mock<IStripeAccountService>().Object,
            new Mock<IPaymentService>().Object,
            new Mock<IConcertService>().Object,
            new Mock<IConcertNotificationService>().Object,
            new Mock<IApplicationNotificationService>().Object,
            new Mock<ITransactionService>().Object,
            TimeProvider.System);
    }

    [Fact]
    public async Task CompleteAsync_ShouldSetStatusToComplete()
    {
        // Arrange
        var application = new OpportunityApplicationEntity { Id = 1, Status = ApplicationStatus.Settled };
        applicationRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(application);

        // Act
        await sut.CompleteAsync(10);

        // Assert
        Assert.Equal(ApplicationStatus.Complete, application.Status);
        applicationRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
