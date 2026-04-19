using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Application;

public class DoorSplitConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;

    public DoorSplitConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IContractRepository contractRepository,
        IConcertRepository concertRepository,
        IManagerRepository<VenueManagerEntity> venueManagerRepository,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository)
    {
        this.deferredConcertService = deferredConcertService;
        this.contractRepository = contractRepository;
        this.concertRepository = concertRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.artistManagerRepository = artistManagerRepository;
    }

    public Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null) =>
        deferredConcertService.InitiateAsync(applicationId);

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishedAsync(int concertId)
    {
        var contract = await contractRepository.GetByConcertIdAsync<DoorSplitContractEntity>(concertId)
            ?? throw new NotFoundException("DoorSplit contract not found");

        var venueManager = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found");

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.CalculateArtistShare(totalRevenue);

        return await deferredConcertService.FinishedAsync(concertId, venueManager, artistManager, artistShare);
    }
}
