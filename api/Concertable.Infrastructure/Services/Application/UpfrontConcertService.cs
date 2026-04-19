using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Services.Application;

public class UpfrontConcertService : IUpfrontConcertService
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IConcertDraftService concertDraftService;

    public UpfrontConcertService(
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

    public async Task InitiateAsync(int applicationId, ManagerEntity payer, ManagerEntity payee, decimal amount, string? paymentMethodId = null)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.AwaitPayment();
        await applicationRepository.SaveChangesAsync();

        await managerPaymentService.PayAsync(payer, payee, amount, applicationId, paymentMethodId);
    }

    public async Task SettleAsync(int applicationId)
    {
        var draftResult = await concertDraftService.CreateAsync(applicationId);

        if (draftResult.IsFailed)
            throw new BadRequestException(draftResult.Errors);
    }

    public async Task FinishedAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Complete();
        await applicationRepository.SaveChangesAsync();
    }
}
