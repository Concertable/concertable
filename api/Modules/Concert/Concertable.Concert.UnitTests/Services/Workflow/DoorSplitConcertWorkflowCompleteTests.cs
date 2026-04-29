using System.Reflection;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Workflow;

public class DoorSplitConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IBookingRepository> bookingRepository;
    private readonly Mock<IContractLoader> contractLookup;
    private readonly DoorSplitConcertWorkflow sut;

    private readonly DoorSplitContract contract = new() { ArtistDoorPercent = 50, PaymentMethod = PaymentMethod.Cash };
    private readonly Guid venueUserId = Guid.NewGuid();
    private readonly Guid artistUserId = Guid.NewGuid();

    public DoorSplitConcertWorkflowCompleteTests()
    {
        deferredConcertService = new Mock<IDeferredConcertService>();
        concertRepository = new Mock<IConcertRepository>();
        bookingRepository = new Mock<IBookingRepository>();
        contractLookup = new Mock<IContractLoader>();

        var booking = BookingFactory.Create(artistUserId, venueUserId);

        sut = new DoorSplitConcertWorkflow(
            deferredConcertService.Object,
            concertRepository.Object,
            bookingRepository.Object,
            new Mock<IPayerLookup>().Object,
            contractLookup.Object,
            new Mock<IConcertPaymentFlow>().Object);

        bookingRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(booking);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
        contractLookup.Setup(l => l.LoadByConcertIdAsync(10)).ReturnsAsync(contract);
    }

    [Fact]
    public async Task FinishAsync_ShouldPassCorrectArtistShareToDeferredService()
    {
        await sut.FinishAsync(10);

        deferredConcertService.Verify(s => s.FinishedAsync(10, venueUserId, artistUserId, 500m), Times.Once);
    }

    internal static class BookingFactory
    {
        public static BookingEntity Create(Guid artistUserId, Guid venueUserId)
        {
            var booking = BookingEntity.Create(applicationId: 1);
            var application = (ApplicationEntity)Activator.CreateInstance(
                typeof(ApplicationEntity),
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
