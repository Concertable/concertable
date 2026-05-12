using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VersusFinishStep : IFinishStep
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IContractLoader contractLoader;
    private readonly ILogger<VersusFinishStep> logger;

    public VersusFinishStep(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IBookingRepository bookingRepository,
        IContractLoader contractLoader,
        ILogger<VersusFinishStep> logger)
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

        var contract = (VersusContract)await contractLoader.LoadByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.Guarantee + (totalRevenue * (contract.ArtistDoorPercent / 100));

        logger.LogDebug(
            "Calculated versus artist share for concert {ConcertId}: {Guarantee} guarantee + ({Revenue} revenue at {Percent}%) = {Share}",
            concertId, contract.Guarantee, totalRevenue, contract.ArtistDoorPercent, artistShare);

        await deferredConcertService.FinishAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
