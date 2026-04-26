using Concertable.Identity.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure;

internal class CustomerPaymentModule : ICustomerPaymentModule
{
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly ICustomerModule customerModule;

    public CustomerPaymentModule(
        [FromKeyedServices(PaymentSession.OnSession)] IPaymentService paymentService,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository,
        ICustomerModule customerModule)
    {
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.customerModule = customerModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid customerUserId,
        Guid payeeUserId,
        decimal amount,
        int referenceId,
        int count,
        string? paymentMethodId,
        CancellationToken ct = default)
    {
        var customer = await customerModule.GetCustomerAsync(customerUserId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(customerUserId)
            ?? throw new NotFoundException($"Payout account not found for payer {customerUserId}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payeeUserId)
            ?? throw new NotFoundException($"Payout account not found for payee {payeeUserId}");

        var stripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var stripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has no Stripe Connect account");

        var resolvedPaymentMethodId = paymentMethodId
            ?? await stripeAccountService.GetPaymentMethodAsync(stripeCustomerId)
            ?? throw new BadRequestException("No payment method provided and no saved payment method found");

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = customer.Email ?? string.Empty,
            StripeCustomerId = stripeCustomerId,
            DestinationStripeId = stripeAccountId,
            Amount = amount,
            Metadata = new Dictionary<string, string>
            {
                ["fromUserId"] = customerUserId.ToString(),
                ["fromUserEmail"] = customer.Email ?? string.Empty,
                ["toUserId"] = payeeUserId.ToString(),
                ["type"] = "concert",
                ["concertId"] = referenceId.ToString(),
                ["quantity"] = count.ToString()
            }
        });
    }
}
