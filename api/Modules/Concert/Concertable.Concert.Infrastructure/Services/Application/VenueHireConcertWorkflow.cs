using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class VenueHireConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IManagerModule managerModule;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IContractRepository contractRepository,
        IOpportunityApplicationRepository applicationRepository,
        IManagerModule managerModule)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.contractRepository = contractRepository;
        this.applicationRepository = applicationRepository;
        this.managerModule = managerModule;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractRepository.GetByApplicationIdAsync<VenueHireContractEntity>(applicationId)
            ?? throw new NotFoundException("VenueHire contract not found");

        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var artistManager = await managerModule.GetByIdAsync(artist.UserId)
            ?? throw new NotFoundException("Artist manager not found");
        var venueManager = await managerModule.GetByIdAsync(venue.UserId)
            ?? throw new NotFoundException("Venue manager not found");

        return await upfrontConcertService.InitiateAsync(applicationId, artistManager, venueManager, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishedAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
