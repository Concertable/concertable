using Concertable.Contract.Abstractions;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class FlatFeeConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractLookup contractLookup;
    private readonly IManagerModule managerModule;

    public FlatFeeConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IOpportunityApplicationRepository applicationRepository,
        IContractLookup contractLookup,
        IManagerModule managerModule)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.applicationRepository = applicationRepository;
        this.contractLookup = contractLookup;
        this.managerModule = managerModule;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var venueManager = await managerModule.GetByIdAsync(venue.UserId)
            ?? throw new NotFoundException("Venue manager not found");
        var artistManager = await managerModule.GetByIdAsync(artist.UserId)
            ?? throw new NotFoundException("Artist manager not found");

        var contract = (FlatFeeContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, venueManager, artistManager, contract.Fee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
