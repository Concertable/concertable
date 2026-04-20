using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Application;

public class VersusConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IManagerModule managerModule;

    public VersusConcertWorkflow(
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
        var contract = await contractRepository.GetByConcertIdAsync<VersusContractEntity>(concertId)
            ?? throw new NotFoundException("Versus contract not found");

        var venueManager = await managerModule.GetVenueManagerByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await managerModule.GetArtistManagerByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found");

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.CalculateArtistShare(totalRevenue);

        return await deferredConcertService.FinishedAsync(concertId, venueManager, artistManager, artistShare);
    }
}
