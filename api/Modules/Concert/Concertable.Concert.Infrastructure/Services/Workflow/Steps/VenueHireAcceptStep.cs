using Concertable.Concert.Application.Workflow.Steps;
using Concertable.Contract.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Shared.Enums;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Steps;

internal class VenueHireAcceptStep : ISimpleAcceptStep
{
    private readonly IImmediateConcertService immediateConcertService;
    private readonly IPayerLookup payerLookup;
    private readonly IContractLoader contractLoader;
    private readonly IApplicationRepository applicationRepository;

    public VenueHireAcceptStep(
        IImmediateConcertService immediateConcertService,
        IPayerLookup payerLookup,
        IContractLoader contractLoader,
        IApplicationRepository applicationRepository)
    {
        this.immediateConcertService = immediateConcertService;
        this.payerLookup = payerLookup;
        this.contractLoader = contractLoader;
        this.applicationRepository = applicationRepository;
    }

    public async Task ExecuteAsync(int applicationId)
    {
        var (venueManagerId, artistManagerId) = await payerLookup.GetManagerIdsAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");
        if (application is not PrepaidApplication prepaid)
            throw new BadRequestException("VenueHire requires a PrepaidApplication");
        var paymentMethodId = prepaid.PaymentMethodId;

        var contract = (VenueHireContract)await contractLoader.LoadByApplicationIdAsync(applicationId);

        await immediateConcertService.ChargeAsync(applicationId, artistManagerId, venueManagerId, contract.HireFee, paymentMethodId, PaymentSession.OffSession);
    }
}
