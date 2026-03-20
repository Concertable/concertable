using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Concertable.Core.Entities.Contracts;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class FlatFeeBookingPaymentService : IBookingPaymentStrategy
{
    private readonly IPaymentService paymentService;
    private readonly ICurrentUser currentUser;
    private readonly IArtistManagerRepository artistManagerRepository;
    private readonly IContractRepository contractRepository;

    public FlatFeeBookingPaymentService(
        IPaymentService paymentService,
        ICurrentUser currentUser,
        IArtistManagerRepository artistManagerRepository,
        IContractRepository contractRepository)
    {
        this.paymentService = paymentService;
        this.currentUser = currentUser;
        this.artistManagerRepository = artistManagerRepository;
        this.contractRepository = contractRepository;
    }

    public async Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId)
    {
        var user = currentUser.Get();

        var contract = await contractRepository.GetByApplicationIdAsync(applicationId) as FlatFeeContractEntity
            ?? throw new NotFoundException("FlatFee contract not found for this application");

        var recipient = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Artist manager not found for this application");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = contract.Fee,
            DestinationStripeId = recipient.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", recipient.Id.ToString() },
                { "type", "booking" },
                { "applicationId", applicationId.ToString() },
                { "contractType", "FlatFee" }
            }
        });
    }
}
