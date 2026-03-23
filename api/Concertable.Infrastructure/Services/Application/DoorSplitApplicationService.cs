using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Application;

public class DoorSplitApplicationService : IApplicationStrategy
{
    private readonly IConcertApplicationValidator applicationValidator;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IContractRepository contractRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IConcertRepository concertRepository;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly IConcertService concertService;
    private readonly IConcertNotificationService notificationService;
    private readonly ITransactionService transactionService;
    private readonly TimeProvider timeProvider;

    public DoorSplitApplicationService(
        IConcertApplicationValidator applicationValidator,
        IConcertApplicationRepository applicationRepository,
        IContractRepository contractRepository,
        IArtistManagerRepository artistManagerRepository,
        IVenueManagerRepository venueManagerRepository,
        IConcertRepository concertRepository,
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
        this.artistManagerRepository = artistManagerRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.concertRepository = concertRepository;
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

        application.Status = ApplicationStatus.AwaitingPayment;
        await applicationRepository.SaveChangesAsync();

        var concert = await concertService.CreateDraftAsync(applicationId);

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found");

        await notificationService.ConcertDraftCreatedAsync(artistManager.Id.ToString(), concert.Id);
    }

    public async Task SettleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Settled;
        await applicationRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(int concertId)
    {
        var application = await applicationRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Application not found");

        var contract = await contractRepository.GetByApplicationIdAsync<DoorSplitContractEntity>(application.Id)
            ?? throw new NotFoundException("DoorSplit contract not found");

        var venueManager = await venueManagerRepository.GetByApplicationIdAsync(application.Id)
            ?? throw new NotFoundException("Venue manager not found");

        var artistManager = await artistManagerRepository.GetByApplicationIdAsync(application.Id)
            ?? throw new NotFoundException("Artist manager not found");

        application.Status = ApplicationStatus.Complete;
        await applicationRepository.SaveChangesAsync();

        var totalRevenue = await concertRepository.GetTotalRevenueByConcertIdAsync(concertId);
        var artistShare = totalRevenue * (contract.ArtistDoorPercent / 100);

        if (venueManager.StripeId is null)
            throw new BadRequestException("Venue manager does not have a Stripe account");

        var paymentMethodId = await stripeAccountService.GetPaymentMethodAsync(venueManager.StripeId);

        var response = await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = venueManager.Email!,
            Amount = artistShare,
            DestinationStripeId = artistManager.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", venueManager.Id.ToString() },
                { "toUserId", artistManager.Id.ToString() },
                { "type", "settlement" },
                { "applicationId", application.Id.ToString() }
            }
        });

        if (response.TransactionId is null)
            throw new InternalServerException("Payment did not return a valid PaymentIntent ID");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            ApplicationId = application.Id,
            FromUserId = venueManager.Id,
            ToUserId = artistManager.Id,
            PaymentIntentId = response.TransactionId,
            Amount = (long)(artistShare * 100),
            Status = TransactionStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });
    }
}
