using Concertable.Concert.Application.Enums;
using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VenueHireConcertWorkflow : IPrepaidConcertWorkflow
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IApplicationRepository applicationRepository;
    private readonly IManagerPaymentModule managerPaymentModule;
    private readonly ICurrentUser currentUser;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IApplicationRepository applicationRepository,
        IManagerPaymentModule managerPaymentModule,
        ICurrentUser currentUser)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.applicationRepository = applicationRepository;
        this.managerPaymentModule = managerPaymentModule;
        this.currentUser = currentUser;
    }

    public Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId, string paymentMethodId) =>
        Task.FromResult<ApplicationEntity>(PrepaidApplication.Create(artistId, opportunityId, paymentMethodId));

    public async Task<Checkout> CheckoutAsync(int opportunityId)
    {
        var venue = await payerLookup.GetVenueByOpportunityIdAsync(opportunityId)
            ?? throw new NotFoundException("Opportunity not found");
        var contract = (VenueHireContract)await contractLoader.LoadByOpportunityIdAsync(opportunityId);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = "applicationApply",
            ["opportunityId"] = opportunityId.ToString()
        };

        var session = await managerPaymentModule.CreateSetupSessionAsync(currentUser.GetId(), metadata);
        return new Checkout(PaymentTiming.Authorize, new FlatPayment(contract.HireFee), venue, session);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        if (application is not PrepaidApplication prepaid)
            throw new BadRequestException("VenueHire requires a PrepaidApplication");
        var paymentMethodId = prepaid.PaymentMethodId;

        var contract = (VenueHireContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, artistManagerId, venueManagerId, contract.HireFee, paymentMethodId, PaymentSession.OffSession);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
