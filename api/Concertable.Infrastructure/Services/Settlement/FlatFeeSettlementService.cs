using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Core.Exceptions;

namespace Infrastructure.Services.Settlement;

public class FlatFeeSettlementService : IPayImmediately
{
    private readonly IContractRepository contractRepository;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IConcertApplicationRepository applicationRepository;
    private readonly IConcertRepository concertRepository;

    public FlatFeeSettlementService(
        IContractRepository contractRepository,
        IVenueManagerRepository venueManagerRepository,
        IArtistManagerRepository artistManagerRepository,
        IPaymentService paymentService,
        IStripeAccountService stripeAccountService,
        IConcertApplicationRepository applicationRepository,
        IConcertRepository concertRepository)
    {
        this.contractRepository = contractRepository;
        this.venueManagerRepository = venueManagerRepository;
        this.artistManagerRepository = artistManagerRepository;
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
        this.applicationRepository = applicationRepository;
        this.concertRepository = concertRepository;
    }

    public async Task SettleAsync(int concertId)
    {
        var concert = await concertRepository.GetByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");

        var contract = await contractRepository.GetByConcertIdAsync<FlatFeeContractEntity>(concertId)
            ?? throw new NotFoundException("FlatFee contract not found for this concert");

        var venueManager = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");

        var artistManager = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found for this concert");

        var paymentMethodId = await stripeAccountService.GetPaymentMethodAsync(venueManager.StripeId!);

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
                { "concertId", concertId.ToString() },
                { "contractType", "FlatFee" }
            }
        });

        var application = await applicationRepository.GetByIdAsync(concert.ApplicationId)
            ?? throw new NotFoundException("Application not found");

        application.Status = ApplicationStatus.Settled;

        await applicationRepository.SaveChangesAsync();
    }
}
