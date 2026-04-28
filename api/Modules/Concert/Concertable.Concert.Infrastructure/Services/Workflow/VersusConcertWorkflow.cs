using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VersusConcertWorkflow : IConcertWorkflowStrategy
{
    private readonly IDeferredConcertService deferredConcertService;
    private readonly IConcertRepository concertRepository;
    private readonly IBookingRepository bookingRepository;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLookup contractLookup;
    private readonly IConcertPaymentFlow paymentFlow;

    public VersusConcertWorkflow(
        IDeferredConcertService deferredConcertService,
        IConcertRepository concertRepository,
        IBookingRepository bookingRepository,
        IPayerLookup payerLookup,
        IContractLookup contractLookup,
        [FromKeyedServices(PaymentSession.OffSession)] IConcertPaymentFlow paymentFlow)
    {
        this.deferredConcertService = deferredConcertService;
        this.concertRepository = concertRepository;
        this.bookingRepository = bookingRepository;
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
        var contract = (VersusContract)await contractLookup.GetByApplicationIdAsync(applicationId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationAccept",
            ["applicationId"] = applicationId.ToString()
        };

        var session = await paymentFlow.CreateSessionAsync(venueManagerId, metadata);
        return new AcceptCheckout(
            PaymentTiming.Deferred,
            new GuaranteedDoorPayment(contract.Guarantee, contract.ArtistDoorPercent),
            artist,
            session);
    }

    public async Task<IAcceptOutcome> InitiateAsync(int applicationId, string? paymentMethodId = null)
    {
        var venueManagerId = await payerLookup.GetVenueManagerIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        return await deferredConcertService.InitiateAsync(applicationId, venueManagerId, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        deferredConcertService.SettleAsync(bookingId);

    public async Task<IFinishOutcome> FinishAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");

        var contract = (VersusContract)await contractLookup.GetByConcertIdAsync(concertId);
        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = contract.Guarantee + (totalRevenue * (contract.ArtistDoorPercent / 100));

        return await deferredConcertService.FinishedAsync(
            concertId,
            booking.Application.Opportunity.Venue.UserId,
            booking.Application.Artist.UserId,
            artistShare);
    }
}
