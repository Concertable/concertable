using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Application;

public class FlatFeeConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IConcertService concertService;
    private readonly IApplicationNotificationService applicationNotificationService;

    public FlatFeeConcertWorkflow(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IManagerRepository<VenueManagerEntity> venueManagerRepository,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository,
        IManagerPaymentService managerPaymentService,
        IConcertService concertService,
        IApplicationNotificationService applicationNotificationService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.contractRepository = contractRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.managerPaymentService = managerPaymentService;
        this.concertService = concertService;
        this.applicationNotificationService = applicationNotificationService;
    }

    public async Task InitiateAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = await contractRepository.GetByApplicationIdAsync<FlatFeeContractEntity>(applicationId)
            ?? throw new NotFoundException("FlatFee contract not found");

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.AwaitingPayment;
        await applicationRepository.SaveChangesAsync();

        await managerPaymentService.PayAsync(venueManager, artistManager, contract.Fee, applicationId);
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.Accepted;
        await applicationRepository.SaveChangesAsync();

        var concertId = await concertService.CreateDraftAsync(applicationId);
        await applicationNotificationService.ApplicationAcceptedAsync(artistManager.Id.ToString(), concertId);
    }

    public async Task FinishedAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();
    }
}
