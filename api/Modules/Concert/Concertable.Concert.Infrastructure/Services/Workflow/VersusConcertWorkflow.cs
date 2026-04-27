using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VersusConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLookup contractLookup;

    public VersusConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IConcertBookingRepository bookingRepository,
        IPayerLookup payerLookup,
        IContractLookup contractLookup)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.payerLookup = payerLookup;
        this.contractLookup = contractLookup;
    }

    public async Task<AcceptPreview> PreviewAsync(int applicationId)
    {
        var artist = await payerLookup.GetArtistAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (VersusContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        return new AcceptPreview(
            PaymentTiming.Deferred,
            new GuaranteedDoorPayment(contract.Guarantee, contract.ArtistDoorPercent),
            artist);
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        return await deferredConcertService.InitiateAsync(applicationId, venueManagerId, paymentMethodId);
    }

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
