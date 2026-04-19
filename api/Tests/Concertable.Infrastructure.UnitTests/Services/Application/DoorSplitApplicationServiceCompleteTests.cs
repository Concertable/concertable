using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class DoorSplitConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IManagerRepository<VenueManagerEntity>> venueManagerRepository;
    private readonly Mock<IManagerRepository<ArtistManagerEntity>> artistManagerRepository;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly DoorSplitConcertWorkflow sut;

    private readonly DoorSplitContractEntity contract = DoorSplitContractEntity.Create(50, PaymentMethod.Cash);
    private readonly VenueManagerEntity venueManager;
    private readonly ArtistManagerEntity artistManager;

    public DoorSplitConcertWorkflowCompleteTests()
    {
        venueManager = VenueManagerEntity.Create("venue@test.com", string.Empty);
        artistManager = ArtistManagerEntity.Create("artist@test.com", string.Empty);

        deferredConcertService = new Mock<IDeferredConcertService>();
        contractRepository = new Mock<IContractRepository>();
        venueManagerRepository = new Mock<IManagerRepository<VenueManagerEntity>>();
        artistManagerRepository = new Mock<IManagerRepository<ArtistManagerEntity>>();
        concertRepository = new Mock<IConcertRepository>();

        sut = new DoorSplitConcertWorkflow(
            deferredConcertService.Object,
            contractRepository.Object,
            concertRepository.Object,
            venueManagerRepository.Object,
            artistManagerRepository.Object);

        contractRepository.Setup(r => r.GetByConcertIdAsync<DoorSplitContractEntity>(10)).ReturnsAsync(contract);
        venueManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(venueManager);
        artistManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(artistManager);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
    }

    [Fact]
    public async Task FinishedAsync_ShouldPassCorrectArtistShareToDeferredService()
    {
        // 50% of 1000 = 500
        await sut.FinishedAsync(10);

        deferredConcertService.Verify(s => s.FinishedAsync(10, venueManager, artistManager, 500m), Times.Once);
    }
}
