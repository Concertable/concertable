using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;
    private readonly ILogger<UpfrontConcertService> logger;

    public UpfrontConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        [FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService,
        ILogger<UpfrontConcertService> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
        this.logger = logger;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string paymentMethodId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var booking = await bookingService.CreateStandardAsync(applicationId);

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = booking.Id.ToString(),
            ["fromUserId"] = payerId.ToString(),
            ["toUserId"] = payeeId.ToString()
        };

        logger.LogInformation(
            "Accepting application {ApplicationId} (booking {BookingId}): charging {Amount} {Currency} from {PayerId} to {PayeeId}",
            applicationId, booking.Id, amount, "GBP", payerId, payeeId);

        var payment = await paymentFlow.PayAsync(payerId, payeeId, amount, paymentMethodId, settlementMetadata);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);

        return new ImmediateAcceptOutcome(payment.Value);
    }

    public async Task SettleAsync(int bookingId)
    {
        var draftResult = await concertDraftService.CreateAsync(bookingId);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task<IFinishOutcome> FinishedAsync(int concertId)
    {
        await bookingService.CompleteByConcertIdAsync(concertId);
        return new ImmediateFinishOutcome();
    }
}
