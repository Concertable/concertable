using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VenueHireConcertWorkflow : IConcertWorkflow
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IConcertPaymentFlow paymentFlow;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        [FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow paymentFlow)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.paymentFlow = paymentFlow;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var venue = await payerLookup.GetVenueAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var artistManagerId = await payerLookup.GetArtistManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var contract = (VenueHireContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString(),
            ["amount"] = ((long)(contract.HireFee * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await paymentFlow.CreateSessionAsync(artistManagerId, metadata);
        return new AcceptCheckout(PaymentTiming.Immediate, new FlatPayment(contract.HireFee), venue, session);
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
