using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class CustomerPaymentService : ICustomerPaymentService
{
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;

    public CustomerPaymentService(IPaymentService paymentService, IStripeAccountService stripeAccountService)
    {
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<PaymentResult> PayAsync(CustomerEntity payer, ManagerEntity payee, int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var resolvedPaymentMethodId = paymentMethodId
            ?? await stripeAccountService.GetPaymentMethodAsync(payer.StripeCustomerId)
            ?? throw new BadRequestException("No payment method provided and no saved payment method found");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = payer.Email,
            StripeCustomerId = payer.StripeCustomerId,
            Amount = price * quantity,
            DestinationStripeId = payee.StripeAccountId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", payer.Id.ToString() },
                { "fromUserEmail", payer.Email },
                { "toUserId", payee.Id.ToString() },
                { "type", "concert" },
                { "concertId", concertId.ToString() },
                { "quantity", quantity.ToString() }
            }
        });
    }
}
