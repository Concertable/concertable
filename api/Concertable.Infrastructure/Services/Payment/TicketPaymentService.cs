using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Core.Enums;

namespace Infrastructure.Services.Payment;

public class TicketPaymentService : ITicketPaymentService
{
    private readonly IPaymentService paymentService;
    private readonly ICurrentUser currentUser;
    private readonly IPaymentRecipientResolverFactory resolverFactory;

    public TicketPaymentService(
        IPaymentService paymentService,
        ICurrentUser currentUser,
        IPaymentRecipientResolverFactory resolverFactory)
    {
        this.paymentService = paymentService;
        this.currentUser = currentUser;
        this.resolverFactory = resolverFactory;
    }

    public async Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price, ContractType contractType)
    {
        var user = currentUser.Get();
        var recipientResolver = resolverFactory.Create(contractType);
        var recipient = await recipientResolver.ResolveAsync(concertId);

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
