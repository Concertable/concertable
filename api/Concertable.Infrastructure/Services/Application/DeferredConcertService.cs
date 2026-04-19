using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Services.Application;

public class DeferredConcertService : IDeferredConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IConcertDraftService concertDraftService;

    public DeferredConcertService(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IManagerPaymentService managerPaymentService,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.managerPaymentService = managerPaymentService;
        this.concertDraftService = concertDraftService;
    }

    public async Task InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.StorePaymentMethod(paymentMethodId);
        await applicationRepository.SaveChangesAsync();

        var draftResult = await concertDraftService.CreateDraftAsync(applicationId);

        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Complete();
        await applicationRepository.SaveChangesAsync();
    }

    public async Task FinishedAsync(int concertId, ManagerEntity payer, ManagerEntity payee, decimal amount)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.AwaitPayment();
        await applicationRepository.SaveChangesAsync();

        await managerPaymentService.PayAsync(payer, payee, amount, application.Id, application.PaymentMethodId);
    }
}
