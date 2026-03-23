using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Application;

public class FlatFeeApplicationService : IApplicationStrategy
{
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService notificationService;

    public FlatFeeApplicationService(
        IConcertApplicationValidator applicationValidator,
        IConcertApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IVenueManagerRepository venueManagerRepository,
        IArtistManagerRepository artistManagerRepository,
        IStripeAccountService stripeAccountService,
        IPaymentService paymentService,
        IConcertService concertService,
        IConcertNotificationService notificationService)
    {
        this.applicationValidator = applicationValidator;
        this.applicationRepository = applicationRepository;
        this.contractRepository = contractRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.stripeAccountService = stripeAccountService;
        this.paymentService = paymentService;
        this.concertService = concertService;
        this.notificationService = notificationService;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var result = await applicationValidator.CanAcceptAsync(applicationId);

        if (!result.IsValid)
            throw new BadRequestException(result.Errors);

        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var contract = await contractRepository.GetByApplicationIdAsync<FlatFeeContractEntity>(applicationId)
            ?? throw new NotFoundException("FlatFee contract not found");

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.AwaitingPayment;
        await applicationRepository.SaveChangesAsync();

        if (venueManager.StripeId is null)
            throw new BadRequestException("Venue manager does not have a Stripe account");

        var paymentMethodId = await stripeAccountService.GetPaymentMethodAsync(venueManager.StripeId);

        await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = venueManager.Email!,
            Amount = contract.Fee,
            DestinationStripeId = artistManager.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", venueManager.Id.ToString() },
                { "toUserId", artistManager.Id.ToString() },
                { "type", "settlement" },
                { "applicationId", applicationId.ToString() }
            }
        });
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Settled;
        await applicationRepository.SaveChangesAsync();

        var concert = await concertService.CreateDraftAsync(applicationId);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await notificationService.ConcertDraftCreatedAsync(artistManager.Id.ToString(), concert.Id);
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();
    }
}
