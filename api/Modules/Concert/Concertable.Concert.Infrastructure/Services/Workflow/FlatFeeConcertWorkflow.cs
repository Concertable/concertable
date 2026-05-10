using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class FlatFeeConcertWorkflow : IDirectConcertWorkflow
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IEscrowModule escrowModule;
    private readonly IConcertDraftService concertDraftService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly ILogger<FlatFeeConcertWorkflow> logger;

    public FlatFeeConcertWorkflow(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        IEscrowModule escrowModule,
        IConcertDraftService concertDraftService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IManagerPaymentModule managerPaymentModule,
        ILogger<FlatFeeConcertWorkflow> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.escrowModule = escrowModule;
        this.concertDraftService = concertDraftService;
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
        var contract = (FlatFeeContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString()
        };

        var session = await managerPaymentModule.CreateHoldSessionAsync(venueManagerId, contract.Fee, metadata);
        return new Checkout(new FlatPayment(contract.Fee), artist, session, CheckoutLabels.Charge);
    }

    public async Task AcceptAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (FlatFeeContract)await contractLoader.LoadByApplicationIdAsync(applicationId);
        var booking = await bookingService.CreateStandardAsync(applicationId);

        var paymentIntentId = await managerPaymentModule.FindHeldIntentAsync(venueManagerId, applicationId);

        logger.LogInformation(
            "Accepting application {ApplicationId} (booking {BookingId}): binding pre-authorised PaymentIntent {PaymentIntentId} for {Amount} {Currency} from {PayerId} on behalf of {PayeeId}",
            applicationId, booking.Id, paymentIntentId, contract.Fee, "GBP", venueManagerId, artistManagerId);

        var bind = await escrowModule.BindAsync(venueManagerId, artistManagerId, contract.Fee, paymentIntentId, booking.Id);
        if (bind.IsFailed)
            throw new BadRequestException(bind.Errors);
    }

    public async Task SettleAsync(int bookingId)
    {
        var draftResult = await concertDraftService.CreateAsync(bookingId);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task FinishAsync(int concertId)
    {
        var booking = await bookingService.CompleteByConcertIdAsync(concertId);

        var release = await escrowModule.ReleaseByBookingIdAsync(booking.Id);
        if (release.IsFailed)
            throw new BadRequestException(release.Errors);
    }
}
