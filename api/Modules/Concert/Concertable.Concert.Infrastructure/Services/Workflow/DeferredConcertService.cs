using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;

    public DeferredConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        [FromKeyedServices(PaymentSession.OffSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
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
            ["bookingId"] = booking.Id.ToString()
        };

        var storedPaymentMethodId = booking.PaymentMethodId
            ?? throw new BadRequestException("Booking has no stored payment method");

        var payment = await paymentFlow.PayAsync(payerId, payeeId, amount, storedPaymentMethodId, settlementMetadata);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);
        return new DeferredFinishOutcome(payment.Value);
    }

    public Task SettleAsync(int bookingId) =>
        bookingService.CompleteAsync(bookingId);
}
