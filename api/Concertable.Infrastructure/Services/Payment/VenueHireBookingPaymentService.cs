using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Concertable.Core.Entities.Contracts;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class VenueHireBookingPaymentService : IBookingPaymentStrategy
{
    private readonly IPaymentService paymentService;
    private readonly ICurrentUser currentUser;
    private readonly IVenueManagerRepository venueManagerRepository;
    private readonly IContractRepository contractRepository;

    public VenueHireBookingPaymentService(
        IPaymentService paymentService,
        ICurrentUser currentUser,
        IVenueManagerRepository venueManagerRepository,
        IContractRepository contractRepository)
    {
        this.paymentService = paymentService;
        this.currentUser = currentUser;
        this.venueManagerRepository = venueManagerRepository;
        this.contractRepository = contractRepository;
    }

    public async Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId)
    {
        var user = currentUser.Get();

        var contract = await contractRepository.GetByApplicationIdAsync(applicationId) as VenueHireContractEntity
            ?? throw new NotFoundException("VenueHire contract not found for this application");

        var recipient = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Venue manager not found for this application");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = contract.HireFee,
            DestinationStripeId = recipient.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", recipient.Id.ToString() },
                { "type", "booking" },
                { "applicationId", applicationId.ToString() },
                { "contractType", "VenueHire" }
            }
        });
    }
}
