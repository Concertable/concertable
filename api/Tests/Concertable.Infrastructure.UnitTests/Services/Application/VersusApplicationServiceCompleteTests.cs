using System.Reflection;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Concertable.Contract.Contracts;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Application;

public class VersusConcertWorkflowCompleteTests
{
    private readonly Mock<IDeferredConcertService> deferredConcertService;
    private readonly Mock<IConcertRepository> concertRepository;
    private readonly Mock<IConcertBookingRepository> bookingRepository;
    private readonly Mock<IContractLookup> contractLookup;
    private readonly VersusConcertWorkflow sut;

    private readonly VersusContract contract = new() { Guarantee = 200, ArtistDoorPercent = 50, PaymentMethod = PaymentMethod.Cash };
    private readonly Guid venueUserId = Guid.NewGuid();
    private readonly Guid artistUserId = Guid.NewGuid();

    public VersusConcertWorkflowCompleteTests()
    {
        deferredConcertService = new Mock<IDeferredConcertService>();
        concertRepository = new Mock<IConcertRepository>();
        bookingRepository = new Mock<IConcertBookingRepository>();
        contractLookup = new Mock<IContractLookup>();

        var booking = BookingFactory.Create(artistUserId, venueUserId);

        sut = new VersusConcertWorkflow(
            deferredConcertService.Object,
            concertRepository.Object,
            bookingRepository.Object,
            contractLookup.Object);

        bookingRepository.Setup(r => r.GetByConcertIdAsync(10)).ReturnsAsync(booking);
        concertRepository.Setup(r => r.GetTotalRevenueByConcertIdAsync(10)).ReturnsAsync(1000);
        contractLookup.Setup(l => l.GetByConcertIdAsync(10)).ReturnsAsync(contract);
    }

    [Fact]
    public async Task FinishAsync_ShouldPassCorrectArtistShareToDeferredService()
    {
        // guarantee 200 + 50% of 1000 = 700
        await sut.FinishAsync(10);

        deferredConcertService.Verify(s => s.FinishedAsync(10, venueUserId, artistUserId, 700m), Times.Once);
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
