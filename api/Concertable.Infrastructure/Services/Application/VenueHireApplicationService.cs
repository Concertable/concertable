using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Application;

public class VenueHireApplicationService : IApplicationStrategy
{
    private readonly IOpportunityApplicationValidator applicationValidator;
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService concertNotificationService;
    private readonly IApplicationNotificationService applicationNotificationService;
    private readonly ITransactionService transactionService;
    private readonly TimeProvider timeProvider;

    public VenueHireApplicationService(
        IOpportunityApplicationValidator applicationValidator,
        IOpportunityApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IVenueManagerRepository venueManagerRepository,
        IArtistManagerRepository artistManagerRepository,
        IStripeAccountService stripeAccountService,
        IPaymentService paymentService,
        IConcertService concertService,
        IConcertNotificationService concertNotificationService,
        IApplicationNotificationService applicationNotificationService,
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
        this.concertNotificationService = concertNotificationService;
        this.applicationNotificationService = applicationNotificationService;
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

        var contract = await contractRepository.GetByApplicationIdAsync<VenueHireContractEntity>(applicationId)
            ?? throw new NotFoundException("VenueHire contract not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found");

        application.Status = ApplicationStatus.AwaitingPayment;
        await applicationRepository.SaveChangesAsync();

        if (artistManager.StripeId is null)
            throw new BadRequestException("Artist manager does not have a Stripe account");

        var paymentMethodId = await stripeAccountService.GetPaymentMethodAsync(artistManager.StripeId);

        var response = await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = artistManager.Email!,
            Amount = contract.HireFee,
            DestinationStripeId = venueManager.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", artistManager.Id.ToString() },
                { "toUserId", venueManager.Id.ToString() },
                { "type", "settlement" },
                { "applicationId", applicationId.ToString() }
            }
        });

        if (response.TransactionId is null)
            throw new InternalServerException("Payment did not return a valid PaymentIntent ID");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            ApplicationId = applicationId,
            FromUserId = artistManager.Id,
            ToUserId = venueManager.Id,
            PaymentIntentId = response.TransactionId,
            Amount = (long)(contract.HireFee * 100),
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

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found");

        application.Status = ApplicationStatus.Settled;
        await applicationRepository.SaveChangesAsync();

        var concert = await concertService.CreateDraftAsync(applicationId);
        await applicationNotificationService.ApplicationAcceptedAsync(artistManager.Id.ToString(), concert.Id);
        await concertNotificationService.ConcertDraftCreatedAsync(venueManager.Id.ToString(), concert.Id);
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();
    }
}
