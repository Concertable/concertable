using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

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
        [FromKeyedServices("offSession")] IManagerPaymentService managerPaymentService,
        IConcertDraftService concertDraftService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.managerPaymentService = managerPaymentService;
        this.concertDraftService = concertDraftService;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var draftResult = await concertDraftService.CreateAsync(applicationId);

        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);

        return new DeferredAcceptOutcome();
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Complete();
        await applicationRepository.SaveChangesAsync();
    }

    public async Task<IFinishOutcome> FinishedAsync(int concertId, ManagerEntity payer, ManagerEntity payee, decimal amount)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.AwaitPayment();
        await applicationRepository.SaveChangesAsync();

        var payment = await managerPaymentService.PayAsync(payer, payee, amount, application.Id, application.PaymentMethodId);
        return new DeferredFinishOutcome(payment);
    }
}
