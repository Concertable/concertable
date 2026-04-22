using Concertable.Shared.Exceptions;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Services.Application;

public class VenueHireConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IContractRepository contractRepository;
    private readonly IManagerModule managerModule;

    public VenueHireConcertWorkflow(
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
        var contract = await contractRepository.GetByApplicationIdAsync<VenueHireContractEntity>(applicationId)
            ?? throw new NotFoundException("VenueHire contract not found");

        var artistManager = await managerModule.GetArtistManagerByApplicationIdAsync(applicationId);
        var venueManager = await managerModule.GetVenueManagerByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, artistManager, venueManager, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishedAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
