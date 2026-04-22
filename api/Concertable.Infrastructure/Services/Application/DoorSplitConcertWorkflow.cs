using Concertable.Shared.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Services.Application;

internal class DoorSplitConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IManagerModule managerModule;

    public DoorSplitConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IContractRepository contractRepository,
        IConcertRepository concertRepository,
        IManagerModule managerModule)
    {
        this.deferredConcertService = deferredConcertService;
        this.contractRepository = contractRepository;
        this.concertRepository = concertRepository;
        this.managerModule = managerModule;
    }

    public Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null) =>
        deferredConcertService.InitiateAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishedAsync(int concertId)
    {
        var contract = await contractRepository.GetByConcertIdAsync<DoorSplitContractEntity>(concertId)
            ?? throw new NotFoundException("DoorSplit contract not found");

        var venueManager = await managerModule.GetVenueManagerByConcertIdAsync(concertId);
        var artistManager = await managerModule.GetArtistManagerByConcertIdAsync(concertId);

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.CalculateArtistShare(totalRevenue);

        return await deferredConcertService.FinishedAsync(concertId, venueManager, artistManager, artistShare);
    }
}
