using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Concertable.Infrastructure.Calculators;

namespace Concertable.Infrastructure.Services.Application;

public class VersusConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IConcertService concertService;
    private readonly IApplicationNotificationService applicationNotificationService;

    public VersusConcertWorkflow(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository,
        IManagerRepository<VenueManagerEntity> venueManagerRepository,
        IConcertRepository concertRepository,
        IManagerPaymentService managerPaymentService,
        IConcertService concertService,
        IApplicationNotificationService applicationNotificationService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.contractRepository = contractRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.concertRepository = concertRepository;
        this.managerPaymentService = managerPaymentService;
        this.concertService = concertService;
        this.applicationNotificationService = applicationNotificationService;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.AwaitingPayment;
        await applicationRepository.SaveChangesAsync();

        var concertId = await concertService.CreateDraftAsync(applicationId);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await applicationNotificationService.ApplicationAcceptedAsync(artistManager.Id.ToString(), concertId);
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Settled;
        await applicationRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        var contract = await contractRepository.GetByConcertIdAsync<VersusContractEntity>(concertId)
            ?? throw new NotFoundException("Versus contract not found");

        var venueManager = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = VersusCalculator.ArtistShare(contract.Guarantee, totalRevenue, contract.ArtistDoorPercent);

        await managerPaymentService.PayAsync(venueManager, artistManager, artistShare, application.Id);
    }
}
