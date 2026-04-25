using System.Reflection;
using Concertable.Concert.Infrastructure.Services.Application;
using Concertable.Contract.Abstractions;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class DoorSplitConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IConcertBookingRepository> bookingRepository;
    private readonly Mock<IContractLookup> contractLookup;
    private readonly Mock<IManagerModule> managerModule;
    private readonly DoorSplitConcertWorkflow sut;

    private readonly DoorSplitContract contract = new() { ArtistDoorPercent = 50, PaymentMethod = PaymentMethod.Cash };
    private readonly Guid venueUserId = Guid.NewGuid();
    private readonly Guid artistUserId = Guid.NewGuid();
    private readonly ManagerDto venueManager;
    private readonly ManagerDto artistManager;

    public DoorSplitConcertWorkflowCompleteTests()
    {
        venueManager = new ManagerDto { Id = venueUserId, Email = "venue@test.com" };
        artistManager = new ManagerDto { Id = artistUserId, Email = "artist@test.com" };

        deferredConcertService = new Mock<IDeferredConcertService>();
        concertRepository = new Mock<IConcertRepository>();
        bookingRepository = new Mock<IConcertBookingRepository>();
        contractLookup = new Mock<IContractLookup>();
        managerModule = new Mock<IManagerModule>();

        var booking = BookingFactory.Create(artistUserId, venueUserId);

        sut = new DoorSplitConcertWorkflow(
            deferredConcertService.Object,
            concertRepository.Object,
            bookingRepository.Object,
            contractLookup.Object,
            managerModule.Object);

        bookingRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(booking);
        managerModule.Setup(r => r.GetByIdAsync(venueUserId)).ReturnsAsync(venueManager);
        managerModule.Setup(r => r.GetByIdAsync(artistUserId)).ReturnsAsync(artistManager);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
        contractLookup.Setup(l => l.GetByConcertIdAsync(10)).ReturnsAsync(contract);
    }

    [Fact]
    public async Task FinishAsync_ShouldPassCorrectArtistShareToDeferredService()
    {
        // 50% of 1000 = 500
        await sut.FinishAsync(10);

        deferredConcertService.Verify(s => s.FinishedAsync(10, venueManager, artistManager, 500m), Times.Once);
    }

    internal static class BookingFactory
    {
        public static ConcertBookingEntity Create(Guid artistUserId, Guid venueUserId)
        {
            var booking = ConcertBookingEntity.Create(applicationId: 1);
            var application = (OpportunityApplicationEntity)Activator.CreateInstance(
                typeof(OpportunityApplicationEntity),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: null,
                culture: null)!;
            var opportunity = (OpportunityEntity)Activator.CreateInstance(
                typeof(OpportunityEntity),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: null,
                culture: null)!;

            application.Artist = new ArtistReadModel { Id = 1, UserId = artistUserId, Name = "Artist" };
            opportunity.Venue = new VenueReadModel { Id = 1, UserId = venueUserId, Name = "Venue", About = "" };
            application.Opportunity = opportunity;
            booking.Application = application;

            return booking;
        }
    }
}
