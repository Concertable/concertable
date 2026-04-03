using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Application;

public class FlatFeeApplicationService : IApplicationStrategy
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService notificationService;
    private readonly ITransactionService transactionService;
    private readonly TimeProvider timeProvider;

    public FlatFeeApplicationService(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IVenueManagerRepository venueManagerRepository,
        IArtistManagerRepository artistManagerRepository,
        IStripeAccountService stripeAccountService,
        IPaymentService paymentService,
        IConcertService concertService,
        IConcertNotificationService notificationService,
        ITransactionService transactionService,
        TimeProvider timeProvider)
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
        this.transactionService = transactionService;
        this.timeProvider = timeProvider;
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

        var response = await paymentService.ProcessAsync(new TransactionRequest
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

        if (response.TransactionId is null)
            throw new InternalServerException("Payment did not return a valid PaymentIntent ID");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            ApplicationId = applicationId,
            FromUserId = venueManager.Id,
            ToUserId = artistManager.Id,
            PaymentIntentId = response.TransactionId,
            Amount = (long)(contract.Fee * 100),
            Status = TransactionStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.Settled;
        await applicationRepository.SaveChangesAsync();

        var concert = await concertService.CreateDraftAsync(applicationId);
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
