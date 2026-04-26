using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class DeferredConcertService : IDeferredConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IApplicationAcceptHandler acceptHandler;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly IConcertDraftService concertDraftService;

    public DeferredConcertService(
        IOpportunityApplicationValidator applicationValidator,
        IConcertBookingRepository bookingRepository,
        IApplicationAcceptHandler acceptHandler,
        [FromKeyedServices(PaymentSession.OffSession)] IManagerPaymentModule managerPaymentModule,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.bookingRepository = bookingRepository;
        this.acceptHandler = acceptHandler;
        this.managerPaymentModule = managerPaymentModule;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var bookingConcert = ConcertBookingEntity.Create(applicationId);
        await bookingRepository.AddAsync(bookingConcert);

        await acceptHandler.HandleAsync(applicationId, bookingConcert);

        var draftResult = await concertDraftService.CreateAsync(bookingConcert.Id);
        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);

        return new DeferredAcceptOutcome();
    }

    public async Task<IFinishOutcome> FinishedAsync(int concertId, Guid payerUserId, Guid payeeUserId, decimal amount)
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

        var payment = await managerPaymentModule.PayAsync(payerUserId, payeeUserId, amount, settlementMetadata, bookingConcert.PaymentMethodId);
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
