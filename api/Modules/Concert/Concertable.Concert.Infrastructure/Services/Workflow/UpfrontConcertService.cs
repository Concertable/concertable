using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly IConcertDraftService concertDraftService;
    private readonly ILogger<UpfrontConcertService> logger;

    public UpfrontConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        IManagerPaymentModule managerPaymentModule,
        IConcertDraftService concertDraftService,
        ILogger<UpfrontConcertService> logger)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.managerPaymentModule = managerPaymentModule;
        this.concertDraftService = concertDraftService;
        this.logger = logger;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string paymentMethodId, PaymentSession session)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var booking = await bookingService.CreateStandardAsync(applicationId);

        logger.LogInformation(
            "Accepting application {ApplicationId} (booking {BookingId}): holding {Amount} {Currency} from {PayerId} on behalf of {PayeeId}",
            applicationId, booking.Id, amount, "GBP", payerId, payeeId);

        var hold = await managerPaymentModule.HoldAsync(payerId, payeeId, amount, paymentMethodId, session, booking.Id);
        if (hold.IsFailed)
            throw new BadRequestException(hold.Errors);

        return new ImmediateAcceptOutcome(hold.Value);
    }

    public async Task SettleAsync(int bookingId)
    {
        var draftResult = await concertDraftService.CreateAsync(bookingId);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task FinishedAsync(int concertId)
    {
        var booking = await bookingService.CompleteByConcertIdAsync(concertId);

        var escrow = await managerPaymentModule.GetEscrowByBookingIdAsync(booking.Id);
        if (escrow is null)
        {
            logger.LogWarning("No escrow found for booking {BookingId} on concert finish; nothing to release", booking.Id);
            return;
        }

        if (escrow.Status != EscrowStatus.Held)
        {
            logger.LogWarning(
                "Escrow {EscrowId} for booking {BookingId} is {Status}, not Held; skipping release",
                escrow.Id, booking.Id, escrow.Status);
            return;
        }

        var release = await managerPaymentModule.ReleaseAsync(escrow.Id);
        if (release.IsFailed)
            throw new BadRequestException(release.Errors);

        logger.LogInformation(
            "Released escrow {EscrowId} for booking {BookingId} on concert {ConcertId} finish: transfer {TransferId}",
            escrow.Id, booking.Id, concertId, release.Value.TransferId);
    }
}
