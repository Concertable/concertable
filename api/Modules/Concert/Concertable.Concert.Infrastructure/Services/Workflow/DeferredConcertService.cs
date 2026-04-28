using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IApplicationAcceptHandler acceptHandler;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;

    public DeferredConcertService(
        IOpportunityApplicationValidator applicationValidator,
        IConcertBookingRepository bookingRepository,
        IApplicationAcceptHandler acceptHandler,
        [FromKeyedServices(PaymentSession.OffSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.bookingRepository = bookingRepository;
        this.acceptHandler = acceptHandler;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, string? paymentMethodId)
    {
        var appCheck = await applicationValidator.CanAcceptAsync(applicationId);
        if (appCheck.IsFailed)
            throw new BadRequestException(appCheck.Errors);

        var resolvedPaymentMethodId = await paymentFlow.ResolvePaymentMethodAsync(payerId, paymentMethodId);

        var bookingConcert = ConcertBookingEntity.Create(applicationId);
        bookingConcert.StorePaymentMethod(resolvedPaymentMethodId);
        await bookingRepository.AddAsync(bookingConcert);

        await acceptHandler.HandleAsync(applicationId, bookingConcert);

        var draftResult = await concertDraftService.CreateAsync(bookingConcert.Id);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);

        return new DeferredAcceptOutcome();
    }

    public async Task<IFinishOutcome> FinishedAsync(int concertId, Guid payerId, Guid payeeId, decimal amount)
    {
        var bookingConcert = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        bookingConcert.AwaitPayment();
        await bookingRepository.SaveChangesAsync();

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = bookingConcert.Id.ToString()
        };

        var storedPaymentMethodId = bookingConcert.PaymentMethodId
            ?? throw new BadRequestException("Booking has no stored payment method");

        var payment = await paymentFlow.PayAsync(payerId, payeeId, amount, storedPaymentMethodId, settlementMetadata);
        if (payment.IsFailed)
            throw new BadRequestException(payment.Errors);
        return new DeferredFinishOutcome(payment.Value);
    }

    public async Task SettleAsync(int bookingId)
    {
        var bookingConcert = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        bookingConcert.Complete();
        await bookingRepository.SaveChangesAsync();
    }
}
