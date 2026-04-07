using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class VenueTicketPaymentService : ITicketPaymentStrategy
{
    private readonly IPaymentService paymentService;
    private readonly ICurrentUser currentUser;
    private readonly IVenueManagerRepository venueManagerRepository;

    public VenueTicketPaymentService(
        IPaymentService paymentService,
        ICurrentUser currentUser,
        IVenueManagerRepository venueManagerRepository)
    {
        this.paymentService = paymentService;
        this.currentUser = currentUser;
        this.venueManagerRepository = venueManagerRepository;
    }

    public async Task<PaymentResponse> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var user = currentUser.GetEntity();
        var recipient = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");

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
