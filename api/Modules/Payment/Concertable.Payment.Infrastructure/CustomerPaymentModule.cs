using Concertable.Customer.Contracts;
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
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string? paymentMethodId,
        CancellationToken ct = default)
    {
        var customer = await customerModule.GetCustomerAsync(payerId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(payerId)
            ?? throw new NotFoundException($"Payout account not found for payer {payerId}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payeeId)
            ?? throw new NotFoundException($"Payout account not found for payee {payeeId}");

        var stripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var stripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has no Stripe Connect account");

        var resolvedPaymentMethodId = paymentMethodId
            ?? await stripeAccountService.TryGetSavedPaymentMethodAsync(stripeCustomerId)
            ?? throw new BadRequestException("No payment method provided and no saved payment method found");

        var mergedMetadata = new Dictionary<string, string>
        {
            ["fromUserId"] = payerId.ToString(),
            ["fromUserEmail"] = customer.Email ?? string.Empty,
            ["toUserId"] = payeeId.ToString(),
            ["amount"] = ((long)(amount * 100)).ToString()
        }
        .Merge(metadata);

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = customer.Email ?? string.Empty,
            StripeCustomerId = stripeCustomerId,
            DestinationStripeId = stripeAccountId,
            Amount = amount,
            Metadata = mergedMetadata
        });
    }
}
