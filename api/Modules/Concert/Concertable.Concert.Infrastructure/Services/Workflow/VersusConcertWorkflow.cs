using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VersusConcertWorkflow : IDeferredConcertWorkflow
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly ILogger<VersusConcertWorkflow> logger;

    public VersusConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IBookingRepository bookingRepository,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IManagerPaymentModule managerPaymentModule,
        ILogger<VersusConcertWorkflow> logger)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.managerPaymentModule = managerPaymentModule;
        this.logger = logger;
    }

    public Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId) =>
        Task.FromResult<ApplicationEntity>(StandardApplication.Create(artistId, opportunityId));

    public async Task<Checkout> CheckoutAsync(int applicationId)
    {
        var artist = await payerLookup.GetArtistAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (VersusContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Verify,
            ["applicationId"] = applicationId.ToString(),
            ["venueManagerId"] = venueManagerId.ToString()
        };

        var session = await managerPaymentModule.CreateVerifySessionAsync(venueManagerId, metadata);
        return new Checkout(
            new GuaranteedDoorPayment(contract.Guarantee, contract.ArtistDoorPercent),
            artist,
            session,
            CheckoutLabels.Settlement);
    }

    public Task AcceptAsync(int applicationId, string paymentMethodId) =>
        deferredConcertService.RegisterPaymentAsync(applicationId, paymentMethodId);

    public Task VerifyAsync(int applicationId) =>
        deferredConcertService.VerifyAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task FinishAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var contract = (VersusContract)await contractLoader.LoadByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.Guarantee + (totalRevenue * (contract.ArtistDoorPercent / 100));

        logger.LogDebug(
            "Calculated versus artist share for concert {ConcertId}: {Guarantee} {Currency} guarantee + ({Revenue} {Currency} revenue at {Percent}%) = {Share} {Currency}",
            concertId, contract.Guarantee, "GBP", totalRevenue, contract.ArtistDoorPercent, artistShare);

        await deferredConcertService.FinishAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
