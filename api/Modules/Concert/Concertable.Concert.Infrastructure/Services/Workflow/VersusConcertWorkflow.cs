using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class VersusConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IContractLookup contractLookup;

    public VersusConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IConcertBookingRepository bookingRepository,
        IContractLookup contractLookup)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.contractLookup = contractLookup;
    }

    public Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null) =>
        deferredConcertService.InitiateAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var contract = (VersusContract)await contractLookup.GetByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.Guarantee + (totalRevenue * (contract.ArtistDoorPercent / 100));

        return await deferredConcertService.FinishedAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
