using Concertable.User.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Payment.Infrastructure;

internal class ManagerPaymentModule : IManagerPaymentModule
{
    private readonly IPaymentManager paymentManager;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly IUserModule userModule;

    public ManagerPaymentModule(
        IPaymentManager paymentManager,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository,
        IUserModule userModule)
    {
        this.paymentManager = paymentManager;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.userModule = userModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        PaymentSession session,
        CancellationToken ct = default)
    {
        var payer = await userModule.GetManagerByIdAsync(payerId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerId}");

        if (session == PaymentSession.OffSession && !await HasStripeCustomerAsync(payerId, ct))
            throw new BadRequestException("Stripe customer setup is required for off-session payments.");

        return await paymentManager.ChargeAsync(new ChargeRequest
        {
            PayerId = payerId,
            PayerEmail = payer.Email ?? string.Empty,
            PayeeId = payeeId,
            Amount = amount,
            PaymentMethodId = paymentMethodId,
            Metadata = metadata,
            Session = session
        }, ct);
    }

    private async Task<bool> HasStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId is not null;
    }

    public async Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountService.CreatePaymentSessionAsync(stripeCustomerId, metadata, ct);
    }

    public async Task<CheckoutSession> CreateSetupSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountService.CreateSetupSessionAsync(stripeCustomerId, metadata, ct);
    }

    public async Task VerifyAndVoidAsync(Guid payerId, string paymentMethodId, CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        await stripeAccountService.VerifyAndVoidAsync(stripeCustomerId, paymentMethodId, ct);
    }

    private async Task<string> EnsureStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        if (account?.StripeCustomerId is not null)
            return account.StripeCustomerId;

        var manager = await userModule.GetManagerByIdAsync(userId)
            ?? throw new NotFoundException($"Manager not found for userId {userId}");

        await stripeAccountService.ProvisionCustomerAsync(userId, manager.Email ?? string.Empty, ct);

        account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId
            ?? throw new InvalidOperationException("Failed to provision Stripe customer.");
    }
}
