using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class ArtistTicketPaymentService : ITicketPaymentStrategy
{
    private readonly IPaymentService paymentService;
    private readonly ICurrentUser currentUser;
    private readonly IArtistManagerRepository artistManagerRepository;

    public ArtistTicketPaymentService(
        IPaymentService paymentService,
        ICurrentUser currentUser,
        IArtistManagerRepository artistManagerRepository)
    {
        this.paymentService = paymentService;
        this.currentUser = currentUser;
        this.artistManagerRepository = artistManagerRepository;
    }

    public async Task<PaymentResult> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var user = currentUser.GetEntity();
        var recipient = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found for this concert");

        var resolvedPaymentMethodId = paymentMethodId
            ?? user.StripeCustomerId
            ?? throw new BadRequestException("No payment method provided and no saved payment method found");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = user.Email,
            Amount = price * quantity,
            DestinationStripeId = recipient.StripeAccountId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", user.Id.ToString() },
                { "fromUserEmail", user.Email },
                { "toUserId", recipient.Id.ToString() },
                { "type", "concert" },
                { "concertId", concertId.ToString() },
                { "quantity", quantity.ToString() }
            }
        });
    }
}
