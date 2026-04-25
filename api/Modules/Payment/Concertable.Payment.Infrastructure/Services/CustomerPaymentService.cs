using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Services;

internal class CustomerPaymentService : ICustomerPaymentService
{
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;

    public CustomerPaymentService(
        [FromKeyedServices("onSession")] IPaymentService paymentService,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository)
    {
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
    }

    public async Task<Result<PaymentResponse>> PayAsync(CustomerDto payer, ManagerDto payee, int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(payer.Id)
            ?? throw new NotFoundException($"Stripe customer not found for payer {payer.Id}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payee.Id)
            ?? throw new NotFoundException($"Stripe account not found for payee {payee.Id}");

        var stripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var stripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has no Stripe connect account");

        var resolvedPaymentMethodId = paymentMethodId
            ?? await stripeAccountService.GetPaymentMethodAsync(stripeCustomerId)
            ?? throw new BadRequestException("No payment method provided and no saved payment method found");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = payer.Email,
            StripeCustomerId = stripeCustomerId,
            Amount = price * quantity,
            DestinationStripeId = stripeAccountId,
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
