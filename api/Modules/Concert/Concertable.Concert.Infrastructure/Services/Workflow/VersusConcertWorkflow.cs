using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VersusConcertWorkflow : IStandardConcertWorkflow
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly ILogger<VersusConcertWorkflow> logger;

    public VersusConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IBookingRepository bookingRepository,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        [FromKeyedServices(PaymentSession.OffSession)] IConcertPaymentFlow paymentFlow,
        ILogger<VersusConcertWorkflow> logger)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.paymentFlow = paymentFlow;
        this.logger = logger;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var artist = await payerLookup.GetArtistAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (VersusContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString()
        };

        var session = await paymentFlow.CreateSessionAsync(venueManagerId, metadata);
        return new AcceptCheckout(
            PaymentTiming.Deferred,
            new GuaranteedDoorPayment(contract.Guarantee, contract.ArtistDoorPercent),
            artist,
            session);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId)
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

        var contract = (VersusContract)await contractLoader.LoadByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.Guarantee + (totalRevenue * (contract.ArtistDoorPercent / 100));

        logger.LogDebug(
            "Calculated versus artist share for concert {ConcertId}: {Guarantee} {Currency} guarantee + ({Revenue} {Currency} revenue at {Percent}%) = {Share} {Currency}",
            concertId, contract.Guarantee, "GBP", totalRevenue, contract.ArtistDoorPercent, artistShare);

        return await deferredConcertService.FinishedAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
