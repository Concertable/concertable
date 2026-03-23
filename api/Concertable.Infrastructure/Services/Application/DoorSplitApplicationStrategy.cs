using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Application;

public class DoorSplitApplicationStrategy : IApplicationStrategy
{
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService notificationService;

    public DoorSplitApplicationStrategy(
        IConcertApplicationValidator applicationValidator,
        IConcertApplicationRepository applicationRepository,
        IArtistManagerRepository artistManagerRepository,
        IConcertService concertService,
        IConcertNotificationService notificationService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.concertService = concertService;
        this.notificationService = notificationService;
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

        var concert = await concertService.CreateDraftAsync(applicationId);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await notificationService.ConcertDraftCreatedAsync(artistManager.Id.ToString(), concert.Id);
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

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();

        throw new NotImplementedException("DoorSplit payout not yet implemented");
    }
}
