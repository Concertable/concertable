using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VersusConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IManagerRepository<VenueManagerEntity>> venueManagerRepository;
    private readonly Mock<IManagerRepository<ArtistManagerEntity>> artistManagerRepository;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly VersusConcertWorkflow sut;

    private readonly VersusContractEntity contract = VersusContractEntity.Create(200, 50, PaymentMethod.Cash);
    private readonly VenueManagerEntity venueManager;
    private readonly ArtistManagerEntity artistManager;

    public VersusConcertWorkflowCompleteTests()
    {
        venueManager = VenueManagerEntity.Create("venue@test.com", string.Empty);
        artistManager = ArtistManagerEntity.Create("artist@test.com", string.Empty);

        deferredConcertService = new Mock<IDeferredConcertService>();
        contractRepository = new Mock<IContractRepository>();
        venueManagerRepository = new Mock<IManagerRepository<VenueManagerEntity>>();
        artistManagerRepository = new Mock<IManagerRepository<ArtistManagerEntity>>();
        concertRepository = new Mock<IConcertRepository>();

        sut = new VersusConcertWorkflow(
            deferredConcertService.Object,
            contractRepository.Object,
            concertRepository.Object,
            venueManagerRepository.Object,
            artistManagerRepository.Object);

        contractRepository.Setup(r => r.GetByConcertIdAsync<VersusContractEntity>(10)).ReturnsAsync(contract);
        venueManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(venueManager);
        artistManagerRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(artistManager);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
    }

    [Fact]
    public async Task FinishedAsync_ShouldPassCorrectArtistShareToDeferredService()
    {
        // guarantee 200 + 50% of 1000 = 700
        await sut.FinishedAsync(10);

        deferredConcertService.Verify(s => s.FinishedAsync(10, venueManager, artistManager, 700m), Times.Once);
    }
}
