using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class FlatFeeConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLookup contractLookup;
    private readonly IConcertPaymentFlow paymentFlow;

    public FlatFeeConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLookup contractLookup,
        [FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow paymentFlow)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLookup = contractLookup;
        this.paymentFlow = paymentFlow;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var artist = await payerLookup.GetArtistAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (FlatFeeContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString(),
            ["amount"] = ((long)(contract.Fee * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await paymentFlow.CreateSessionAsync(venueManagerId, metadata);
        return new AcceptCheckout(PaymentTiming.Immediate, new FlatPayment(contract.Fee), artist, session);
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = (FlatFeeContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, venueManagerId, artistManagerId, contract.Fee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
