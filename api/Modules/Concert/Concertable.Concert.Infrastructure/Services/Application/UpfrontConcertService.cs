using Concertable.Application.Interfaces.Payment;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IConcertBookingRepository bookingRepository;
    private readonly IApplicationAcceptHandler acceptHandler;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IConcertDraftService concertDraftService;

    public UpfrontConcertService(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IConcertBookingRepository bookingRepository,
        IApplicationAcceptHandler acceptHandler,
        [FromKeyedServices("onSession")] IManagerPaymentService managerPaymentService,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.bookingRepository = bookingRepository;
        this.acceptHandler = acceptHandler;
        this.managerPaymentService = managerPaymentService;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, ManagerDto payer, ManagerDto payee, decimal amount, string? paymentMethodId = null)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);
        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var bookingConcert = ConcertBookingEntity.Create(applicationId);
        bookingConcert.AwaitPayment();
        await bookingRepository.AddAsync(bookingConcert);

        await acceptHandler.HandleAsync(applicationId, bookingConcert);

        var payment = await managerPaymentService.PayAsync(payer, payee, amount, bookingConcert.Id, paymentMethodId);
        return new ImmediateAcceptOutcome(payment);
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
