using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;
using Concertable.Application.Exceptions;
using Concertable.Infrastructure.Calculators;

namespace Concertable.Infrastructure.Services.Application;

public class DoorSplitConcertWorkflow : IConcertWorkflowStrategy
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

    public DoorSplitConcertWorkflow(
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

    public async Task InitiateAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        var concertId = await concertService.CreateDraftAsync(applicationId);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await applicationNotificationService.ApplicationAcceptedAsync(artistManager.Id.ToString(), concertId);
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Complete();
        await applicationRepository.SaveChangesAsync();
    }

    public async Task FinishedAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        var contract = await contractRepository.GetByConcertIdAsync<DoorSplitContractEntity>(concertId)
            ?? throw new NotFoundException("DoorSplit contract not found");

        var venueManager = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found");

        application.AwaitPayment();
        await applicationRepository.SaveChangesAsync();

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = DoorSplitCalculator.ArtistShare(totalRevenue, contract.ArtistDoorPercent);

        await managerPaymentService.PayAsync(venueManager, artistManager, artistShare, application.Id);
    }
}
