using Concertable.Shared.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Services.Application;

internal class FlatFeeConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IManagerModule managerModule;

    public FlatFeeConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IContractRepository contractRepository,
        IManagerModule managerModule)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.contractRepository = contractRepository;
        this.managerModule = managerModule;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractRepository.GetByApplicationIdAsync<FlatFeeContractEntity>(applicationId)
            ?? throw new NotFoundException("FlatFee contract not found");

        var venueManager = await managerModule.GetVenueManagerByApplicationIdAsync(applicationId);
        var artistManager = await managerModule.GetArtistManagerByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, venueManager, artistManager, contract.Fee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishedAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
