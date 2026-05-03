using Concertable.Customer.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Payment.Infrastructure;

internal class CustomerPaymentModule : ICustomerPaymentModule
{
    private readonly IPaymentManager paymentManager;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly ICustomerModule customerModule;

    public CustomerPaymentModule(
        IPaymentManager paymentManager,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository,
        ICustomerModule customerModule)
    {
        this.paymentManager = paymentManager;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.customerModule = customerModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        CancellationToken ct = default)
    {
        var customer = await customerModule.GetCustomerAsync(payerId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        return await paymentManager.ChargeAsync(new ChargeRequest
        {
            PayerId = payerId,
            PayerEmail = customer.Email ?? string.Empty,
            PayeeId = payeeId,
            Amount = amount,
            PaymentMethodId = paymentMethodId,
            Metadata = metadata,
            Session = PaymentSession.OnSession
        }, ct);
    }

    public async Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var customer = await customerModule.GetCustomerAsync(payerId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);

        var mergedMetadata = new Dictionary<string, string>
        {
            ["fromUserId"] = payerId.ToString(),
            ["fromUserEmail"] = customer.Email ?? string.Empty
        }
        .Merge(metadata);

        return await stripeAccountService.CreatePaymentSessionAsync(stripeCustomerId, mergedMetadata, ct);
    }

    private async Task<string> EnsureStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        if (account?.StripeCustomerId is not null)
            return account.StripeCustomerId;

        var customer = await customerModule.GetCustomerAsync(userId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        await stripeAccountService.ProvisionCustomerAsync(userId, customer.Email ?? string.Empty, ct);

        account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId
            ?? throw new InvalidOperationException("Failed to provision Stripe customer.");
    }
}
