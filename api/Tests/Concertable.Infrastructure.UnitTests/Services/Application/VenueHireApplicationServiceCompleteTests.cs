using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VenueHireConcertWorkflowCompleteTests
{
    private readonly Mock<IOpportunityApplicationRepository> applicationRepository;
    private readonly VenueHireConcertWorkflow sut;

    public VenueHireConcertWorkflowCompleteTests()
    {
        applicationRepository = new Mock<IOpportunityApplicationRepository>();
        sut = new VenueHireConcertWorkflow(
            new Mock<IOpportunityApplicationValidator>().Object,
            applicationRepository.Object,
            new Mock<IContractRepository>().Object,
            new Mock<IManagerRepository<ArtistManagerEntity>>().Object,
            new Mock<IManagerRepository<VenueManagerEntity>>().Object,
            new Mock<IManagerPaymentService>().Object,
            new Mock<IConcertService>().Object,
            new Mock<IApplicationNotificationService>().Object);
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
