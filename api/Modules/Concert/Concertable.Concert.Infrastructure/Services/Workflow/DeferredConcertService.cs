using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IBookingRepository bookingRepository;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly IConcertDraftService concertDraftService;
    private readonly ILogger<DeferredConcertService> logger;

    public DeferredConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        IBookingRepository bookingRepository,
        IManagerPaymentModule managerPaymentModule,
        IConcertDraftService concertDraftService,
        ILogger<DeferredConcertService> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.bookingRepository = bookingRepository;
        this.managerPaymentModule = managerPaymentModule;
        this.concertDraftService = concertDraftService;
        this.logger = logger;
    }

    public async Task RegisterPaymentAsync(int applicationId, string paymentMethodId)
    {
        var appCheck = await applicationValidator.CanAcceptAsync(applicationId);
        if (appCheck.IsFailed)
            throw new BadRequestException(appCheck.Errors);

        await bookingService.CreateDeferredAsync(applicationId, paymentMethodId);
    }

    public async Task VerifyAsync(int applicationId)
    {
        var booking = await bookingRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Booking not found for application");

        var draftResult = await concertDraftService.CreateAsync(booking.Id);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task FinishAsync(int concertId, Guid payerId, Guid payeeId, decimal amount)
    {
        var booking = await bookingService.MarkAwaitingPaymentByConcertIdAsync(concertId);
        if (booking is not DeferredBooking deferred)
            throw new BadRequestException("Concert finish requires a DeferredBooking");

        logger.LogInformation(
            "Settling concert {ConcertId} (booking {BookingId}): paying {Amount} {Currency} from {PayerId} to {PayeeId}",
            concertId, booking.Id, amount, "GBP", payerId, payeeId);

        var payment = await managerPaymentModule.PayAsync(payerId, payeeId, amount, deferred.PaymentMethodId, PaymentSession.OffSession, booking.Id);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);
    }

    public Task SettleAsync(int bookingId) =>
        bookingService.CompleteAsync(bookingId);
}
