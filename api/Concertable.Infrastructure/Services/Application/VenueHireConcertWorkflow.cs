using Concertable.Application.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Application;

public class VenueHireConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IContractRepository contractRepository,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository,
        IManagerRepository<VenueManagerEntity> venueManagerRepository)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.contractRepository = contractRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.venueManagerRepository = venueManagerRepository;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractRepository.GetByApplicationIdAsync<VenueHireContractEntity>(applicationId)
            ?? throw new NotFoundException("VenueHire contract not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found");

        return await upfrontConcertService.InitiateAsync(applicationId, artistManager, venueManager, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishedAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
