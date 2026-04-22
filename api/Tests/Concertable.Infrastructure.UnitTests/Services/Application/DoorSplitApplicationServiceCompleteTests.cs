using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class DoorSplitConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IManagerModule> managerModule;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly DoorSplitConcertWorkflow sut;

    private readonly DoorSplitContractEntity contract = DoorSplitContractEntity.Create(50, PaymentMethod.Cash);
    private readonly ManagerDto venueManager = new() { Id = Guid.NewGuid(), Email = "venue@test.com" };
    private readonly ManagerDto artistManager = new() { Id = Guid.NewGuid(), Email = "artist@test.com" };

    public DoorSplitConcertWorkflowCompleteTests()
    {
        deferredConcertService = new Mock<IDeferredConcertService>();
        contractRepository = new Mock<IContractRepository>();
        managerModule = new Mock<IManagerModule>();
        concertRepository = new Mock<IConcertRepository>();

        sut = new DoorSplitConcertWorkflow(
            deferredConcertService.Object,
            contractRepository.Object,
            concertRepository.Object,
            managerModule.Object);

        contractRepository.Setup(r => r.GetByConcertIdAsync<DoorSplitContractEntity>(10)).ReturnsAsync(contract);
        managerModule.Setup(r => r.GetVenueManagerByConcertIdAsync(10)).ReturnsAsync(venueManager);
        managerModule.Setup(r => r.GetArtistManagerByConcertIdAsync(10)).ReturnsAsync(artistManager);
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
