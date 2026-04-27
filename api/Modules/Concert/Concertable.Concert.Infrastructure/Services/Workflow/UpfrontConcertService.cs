using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IApplicationAcceptHandler acceptHandler;
    private readonly IConcertPaymentFlow paymentFlow;
    private readonly IConcertDraftService concertDraftService;

    public UpfrontConcertService(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IConcertBookingRepository bookingRepository,
        IApplicationAcceptHandler acceptHandler,
        [FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow paymentFlow,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.bookingRepository = bookingRepository;
        this.acceptHandler = acceptHandler;
        this.paymentFlow = paymentFlow;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, Guid payeeId, decimal amount, string? paymentMethodId = null)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var resolvedPaymentMethodId = await paymentFlow.ResolvePaymentMethodAsync(payerId, paymentMethodId);

        var bookingConcert = ConcertBookingEntity.Create(applicationId);
        bookingConcert.AwaitPayment();
        await bookingRepository.AddAsync(bookingConcert);

        await acceptHandler.HandleAsync(applicationId, bookingConcert);

        var settlementMetadata = new Dictionary<string, string>
        {
            ["type"] = "settlement",
            ["bookingId"] = bookingConcert.Id.ToString()
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
        var bookingConcert = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        bookingConcert.Complete();
        await bookingRepository.SaveChangesAsync();

        return new ImmediateFinishOutcome();
    }
}
