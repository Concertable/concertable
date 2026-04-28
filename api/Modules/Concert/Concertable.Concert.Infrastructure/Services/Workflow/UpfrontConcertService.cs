using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IApplicationValidator applicationValidator;
    private readonly IBookingService bookingService;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;

    public UpfrontConcertService(
        IApplicationValidator applicationValidator,
        IBookingService bookingService,
        [FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.bookingService = bookingService;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string? paymentMethodId = null)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var resolvedPaymentMethodId = await paymentFlow.ResolvePaymentMethodAsync(payerId, paymentMethodId);

        var booking = await bookingService.CreateAsync(applicationId);

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = booking.Id.ToString()
        };

        var payment = await paymentFlow.PayAsync(payerId, payeeId, amount, resolvedPaymentMethodId, settlementMetadata);
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
