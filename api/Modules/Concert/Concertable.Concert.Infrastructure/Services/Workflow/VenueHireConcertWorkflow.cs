using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VenueHireConcertWorkflow : IConcertWorkflow
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = (VenueHireContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, artistManagerId, venueManagerId, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
