using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class FlatFeeAcceptStep : ISimpleAcceptStep
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IEscrowModule escrowModule;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly ILogger<FlatFeeAcceptStep> logger;

    public FlatFeeAcceptStep(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        IEscrowModule escrowModule,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IManagerPaymentModule managerPaymentModule,
        ILogger<FlatFeeAcceptStep> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.escrowModule = escrowModule;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.managerPaymentModule = managerPaymentModule;
        this.logger = logger;
    }

    public async Task ExecuteAsync(int applicationId)
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

        var bind = await escrowModule.CaptureAsync(venueManagerId, artistManagerId, contract.Fee, paymentIntentId, booking.Id);
        if (bind.IsFailed)
            throw new BadRequestException(bind.Errors);
    }
}
