using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class VenueHireConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractLookup contractLookup;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IOpportunityApplicationRepository applicationRepository,
        IContractLookup contractLookup)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.applicationRepository = applicationRepository;
        this.contractLookup = contractLookup;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var (artist, venue) = await applicationRepository.GetArtistAndVenueByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = (VenueHireContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, artist.UserId, venue.UserId, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
