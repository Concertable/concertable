using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class DoorSplitFinishStep : IFinishStep
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IContractLoader contractLoader;
    private readonly ILogger<DoorSplitFinishStep> logger;

    public DoorSplitFinishStep(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IBookingRepository bookingRepository,
        IContractLoader contractLoader,
        ILogger<DoorSplitFinishStep> logger)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.contractLoader = contractLoader;
        this.logger = logger;
    }

    public async Task ExecuteAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var contract = (DoorSplitContract)await contractLoader.LoadByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = totalRevenue * (contract.ArtistDoorPercent / 100);

        logger.LogDebug(
            "Calculated door-split artist share for concert {ConcertId}: {Revenue} revenue at {Percent}% = {Share}",
            concertId, totalRevenue, contract.ArtistDoorPercent, artistShare);

        await deferredConcertService.FinishAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
