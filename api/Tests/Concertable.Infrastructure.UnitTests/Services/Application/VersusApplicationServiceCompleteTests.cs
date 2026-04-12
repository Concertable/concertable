using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VersusConcertWorkflowCompleteTests
{
    private readonly Mock<IOpportunityApplicationRepository> applicationRepository;
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IManagerRepository<VenueManagerEntity>> venueManagerRepository;
    private readonly Mock<IManagerRepository<ArtistManagerEntity>> artistManagerRepository;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IManagerPaymentService> managerPaymentService;
    private readonly VersusConcertWorkflow sut;

    private readonly OpportunityApplicationEntity application = new() { Id = 5, Status = ApplicationStatus.Confirmed };
    private readonly VersusContractEntity contract = new() { Guarantee = 200, ArtistDoorPercent = 50 };
    private readonly VenueManagerEntity venueManager = new() { Id = Guid.NewGuid(), Email = "venue@test.com", StripeCustomerId = "cus_venue", Role = Role.VenueManager };
    private readonly ArtistManagerEntity artistManager = new() { Id = Guid.NewGuid(), Email = "artist@test.com", StripeAccountId = "acct_artist", Role = Role.ArtistManager };

    public VersusConcertWorkflowCompleteTests()
    {
        applicationRepository = new Mock<IOpportunityApplicationRepository>();
        contractRepository = new Mock<IContractRepository>();
        venueManagerRepository = new Mock<IManagerRepository<VenueManagerEntity>>();
        artistManagerRepository = new Mock<IManagerRepository<ArtistManagerEntity>>();
        concertRepository = new Mock<IConcertRepository>();
        managerPaymentService = new Mock<IManagerPaymentService>();

        sut = new VersusConcertWorkflow(
            new Mock<IOpportunityApplicationValidator>().Object,
            applicationRepository.Object,
            contractRepository.Object,
            artistManagerRepository.Object,
            venueManagerRepository.Object,
            concertRepository.Object,
            managerPaymentService.Object,
            new Mock<IConcertService>().Object,
            new Mock<IApplicationNotificationService>().Object,
            new Mock<IApplicationAcceptHandler>().Object);

        applicationRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(application);
        contractRepository.Setup(r => r.GetByConcertIdAsync<VersusContractEntity>(10)).ReturnsAsync(contract);
        venueManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(venueManager);
        artistManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(artistManager);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
    }

    [Fact]
    public async Task FinishedAsync_ShouldSetStatusToAwaitingPayment()
    {
        await sut.FinishedAsync(10);

        Assert.Equal(ApplicationStatus.AwaitingPayment, application.Status);
        applicationRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task FinishedAsync_ShouldPayCorrectArtistShare()
    {
        // guarantee 200 + 50% of 1000 = 700
        await sut.FinishedAsync(10);

        managerPaymentService.Verify(p => p.PayAsync(venueManager, artistManager, 700m, application.Id), Times.Once);
    }
}
