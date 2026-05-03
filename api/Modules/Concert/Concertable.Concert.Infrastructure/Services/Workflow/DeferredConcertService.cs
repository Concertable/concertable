using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly IConcertDraftService concertDraftService;
    private readonly ILogger<DeferredConcertService> logger;

    public DeferredConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        IManagerPaymentModule managerPaymentModule,
        IConcertDraftService concertDraftService,
        ILogger<DeferredConcertService> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.managerPaymentModule = managerPaymentModule;
        this.concertDraftService = concertDraftService;
        this.logger = logger;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, string paymentMethodId)
    {
        var appCheck = await applicationValidator.CanAcceptAsync(applicationId);
        if (appCheck.IsFailed)
            throw new BadRequestException(appCheck.Errors);

        await managerPaymentModule.VerifyAndVoidAsync(payerId, paymentMethodId);

        var booking = await bookingService.CreateDeferredAsync(applicationId, paymentMethodId);

        var draftResult = await concertDraftService.CreateAsync(booking.Id);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);

        return new DeferredAcceptOutcome();
    }

    public async Task FinishedAsync(int concertId, Guid payerId, Guid payeeId, decimal amount)
    {
        var booking = await bookingService.MarkAwaitingPaymentByConcertIdAsync(concertId);
        if (booking is not DeferredBooking deferred)
            throw new BadRequestException("Concert finish requires a DeferredBooking");

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = booking.Id.ToString(),
            ["fromUserId"] = payerId.ToString(),
            ["toUserId"] = payeeId.ToString()
        };

        logger.LogInformation(
            "Settling concert {ConcertId} (booking {BookingId}): paying {Amount} {Currency} from {PayerId} to {PayeeId}",
            concertId, booking.Id, amount, "GBP", payerId, payeeId);

        var payment = await managerPaymentModule.PayAsync(payerId, payeeId, amount, settlementMetadata, deferred.PaymentMethodId, PaymentSession.OffSession);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);
    }

    public Task SettleAsync(int bookingId) =>
        bookingService.CompleteAsync(bookingId);
}
