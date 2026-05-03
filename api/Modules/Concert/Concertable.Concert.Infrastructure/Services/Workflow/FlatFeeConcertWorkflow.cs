using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class FlatFeeConcertWorkflow : IStandardConcertWorkflow
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IManagerPaymentModule managerPaymentModule;

    public FlatFeeConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IManagerPaymentModule managerPaymentModule)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.managerPaymentModule = managerPaymentModule;
    }

    public Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId) =>
        Task.FromResult<ApplicationEntity>(StandardApplication.Create(artistId, opportunityId));

    public async Task<Checkout> CheckoutAsync(int applicationId)
    {
        var artist = await payerLookup.GetArtistAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (FlatFeeContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString(),
            ["amount"] = ((long)(contract.Fee * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await managerPaymentModule.CreatePaymentSessionAsync(venueManagerId, metadata);
        return new Checkout(PaymentTiming.Immediate, new FlatPayment(contract.Fee), artist, session);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = (FlatFeeContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, venueManagerId, artistManagerId, contract.Fee, paymentMethodId, PaymentSession.OnSession);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
