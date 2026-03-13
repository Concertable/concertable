using Application.DTOs;
using Application.Requests;
using Application.Interfaces;
using Core.Exceptions;
using Application.Responses;

namespace Infrastructure.Services;

public class UserPaymentService : IUserPaymentService
{
    private readonly IPaymentService paymentService;
    private readonly IUserService userService;
    private readonly IConcertApplicationService applicationService;
    private readonly ICurrentUser currentUser;

    public UserPaymentService(
        IPaymentService paymentService,
        IUserService userService,
        IConcertApplicationService applicationService,
        ICurrentUser currentUser)
    {
        this.paymentService = paymentService;
        this.userService = userService;
        this.applicationService = applicationService;
        this.currentUser = currentUser;
    }

    public async Task<PaymentResponse> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId, decimal price)
    {
        var user = currentUser.Get();
        var toUser = await userService.GetByConcertIdAsync(concertId);

        var transactionRequestDto = new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = price * quantity,
            DestinationStripeId = toUser.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUser.Id.ToString() },
                { "type", "concert" },
                { "concertId", concertId.ToString() },
                { "quantity", quantity.ToString() }
            }
        };

        return await paymentService.ProcessAsync(transactionRequestDto);
    }

    public async Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId)
    {
        var user = currentUser.Get();
        var toUser = await userService.GetByApplicationIdAsync(applicationId);
        var pay = await applicationService.GetOpportunityPayByIdAsync(applicationId);

        var transactionRequestDto = new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = pay,
            DestinationStripeId = toUser.StripeId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", toUser.Id.ToString() },
                { "type", "application" },
                { "applicationId", applicationId.ToString() }
            }
        };

        return await paymentService.ProcessAsync(transactionRequestDto);
    }
}
