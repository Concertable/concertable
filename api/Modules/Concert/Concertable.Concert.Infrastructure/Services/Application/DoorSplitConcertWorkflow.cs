using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class DoorSplitConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IManagerModule managerModule;

    public DoorSplitConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IContractRepository contractRepository,
        IConcertRepository concertRepository,
        IConcertBookingRepository bookingRepository,
        IManagerModule managerModule)
    {
        this.deferredConcertService = deferredConcertService;
        this.contractRepository = contractRepository;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.managerModule = managerModule;
    }

    public Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null) =>
        deferredConcertService.InitiateAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishedAsync(int concertId)
    {
        var contract = await contractRepository.GetByConcertIdAsync<DoorSplitContractEntity>(concertId)
            ?? throw new NotFoundException("DoorSplit contract not found");

        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var venueManager = await managerModule.GetByIdAsync(booking.Application.Opportunity.Venue.UserId)
            ?? throw new NotFoundException("Venue manager not found");
        var artistManager = await managerModule.GetByIdAsync(booking.Application.Artist.UserId)
            ?? throw new NotFoundException("Artist manager not found");

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.CalculateArtistShare(totalRevenue);

        return await deferredConcertService.FinishedAsync(concertId, venueManager, artistManager, artistShare);
    }
}
