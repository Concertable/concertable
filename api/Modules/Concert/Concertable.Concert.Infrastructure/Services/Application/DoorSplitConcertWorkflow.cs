using Concertable.Contract.Abstractions;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class DoorSplitConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IContractLookup contractLookup;
    private readonly IManagerModule managerModule;

    public DoorSplitConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IConcertBookingRepository bookingRepository,
        IContractLookup contractLookup,
        IManagerModule managerModule)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.contractLookup = contractLookup;
        this.managerModule = managerModule;
    }

    public Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null) =>
        deferredConcertService.InitiateAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var venueManager = await managerModule.GetByIdAsync(booking.Application.Opportunity.Venue.UserId)
            ?? throw new NotFoundException("Venue manager not found");
        var artistManager = await managerModule.GetByIdAsync(booking.Application.Artist.UserId)
            ?? throw new NotFoundException("Artist manager not found");

        var contract = (DoorSplitContract)await contractLookup.GetByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = totalRevenue * (contract.ArtistDoorPercent / 100);

        return await deferredConcertService.FinishedAsync(concertId, venueManager, artistManager, artistShare);
    }
}
