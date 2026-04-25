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
        [FromKeyedServices("offSession")] IManagerPaymentModule managerPaymentModule,
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

    public async Task<IFinishOutcome> FinishedAsync(int concertId, ManagerDto payer, ManagerDto payee, decimal amount)
    {
        var bookingConcert = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        bookingConcert.AwaitPayment();
        await bookingRepository.SaveChangesAsync();

        var payment = await managerPaymentModule.PayAsync(payer, payee, amount, bookingConcert.Id, bookingConcert.PaymentMethodId);
        return new DeferredFinishOutcome(payment);
    }

    public async Task SettleAsync(int bookingId)
    {
        var bookingConcert = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");

        bookingConcert.Complete();
        await bookingRepository.SaveChangesAsync();
    }
}
