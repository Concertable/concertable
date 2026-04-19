using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Services.Application;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VenueHireConcertWorkflowCompleteTests
{
    private readonly Mock<IUpfrontConcertService> upfrontConcertService;
    private readonly VenueHireConcertWorkflow sut;

    public VenueHireConcertWorkflowCompleteTests()
    {
        upfrontConcertService = new Mock<IUpfrontConcertService>();
        sut = new VenueHireConcertWorkflow(
            upfrontConcertService.Object,
            new Mock<IContractRepository>().Object,
            new Mock<IManagerRepository<ArtistManagerEntity>>().Object,
            new Mock<IManagerRepository<VenueManagerEntity>>().Object);
    }

    [Fact]
    public async Task FinishedAsync_ShouldDelegateToUpfrontConcertService()
    {
        await sut.FinishedAsync(10);

        upfrontConcertService.Verify(s => s.FinishedAsync(10), Times.Once);
    }
}
