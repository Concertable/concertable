using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;
    private readonly ILogger<DeferredConcertService> logger;

    public DeferredConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        [FromKeyedServices(PaymentSession.OffSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService,
        ILogger<DeferredConcertService> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
        this.logger = logger;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, string? paymentMethodId)
    {
        var appCheck = await applicationValidator.CanAcceptAsync(applicationId);
        if (appCheck.IsFailed)
            throw new BadRequestException(appCheck.Errors);

        var resolvedPaymentMethodId = await paymentFlow.ResolvePaymentMethodAsync(payerId, paymentMethodId);

        var booking = await bookingService.CreateAsync(applicationId, resolvedPaymentMethodId);

        var draftResult = await concertDraftService.CreateAsync(booking.Id);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);

        return new DeferredAcceptOutcome();
    }

    public async Task<IFinishOutcome> FinishedAsync(int concertId, Guid payerId, Guid payeeId, decimal amount)
    {
        var booking = await bookingService.MarkAwaitingPaymentByConcertIdAsync(concertId);

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = booking.Id.ToString(),
            ["fromUserId"] = payerId.ToString(),
            ["toUserId"] = payeeId.ToString()
        };

        var resolvedPaymentMethodId = await paymentFlow.ResolvePaymentMethodAsync(payerId, booking.PaymentMethodId);

        logger.LogInformation(
            "Settling concert {ConcertId} (booking {BookingId}): paying {Amount} {Currency} from {PayerId} to {PayeeId}",
            concertId, booking.Id, amount, "GBP", payerId, payeeId);

        var payment = await paymentFlow.PayAsync(payerId, payeeId, amount, resolvedPaymentMethodId, settlementMetadata);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);

        return new DeferredFinishOutcome(payment.Value);
    }

    public Task SettleAsync(int bookingId) =>
        bookingService.CompleteAsync(bookingId);
}
