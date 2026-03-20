using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

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

    public async Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price)
    {
        var user = currentUser.Get();
        var recipient = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = user.Email,
            Amount = price * quantity,
            DestinationStripeId = recipient.StripeId,
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
