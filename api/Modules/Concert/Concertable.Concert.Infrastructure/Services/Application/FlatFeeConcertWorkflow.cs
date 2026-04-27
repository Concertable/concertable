using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal class FlatFeeConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractLookup contractLookup;

    public FlatFeeConcertWorkflow(
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

        var contract = (FlatFeeContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, venue.UserId, artist.UserId, contract.Fee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
