using Concertable.Concert.Application.Responses;
using Concertable.Contract.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class VenueHireConcertWorkflow : IConcertWorkflow, IApplyWithPaymentMethod, IAcceptByConfirmation
{
    private readonly IUpfrontConcertService upfrontConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IApplicationRepository applicationRepository;

    public VenueHireConcertWorkflow(
        IUpfrontConcertService upfrontConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IApplicationRepository applicationRepository)
    {
        this.upfrontConcertService = upfrontConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.applicationRepository = applicationRepository;
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        var paymentMethodId = application.PaymentMethodId
            ?? throw new BadRequestException("VenueHire application has no payment method stored");

        var contract = (VenueHireContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        return await upfrontConcertService.InitiateAsync(applicationId, artistManagerId, venueManagerId, contract.HireFee, paymentMethodId);
    }

    public Task SettleAsync(int bookingId) =>
        upfrontConcertService.SettleAsync(bookingId);

    public Task<IFinishOutcome> FinishAsync(int concertId) =>
        upfrontConcertService.FinishedAsync(concertId);
}
