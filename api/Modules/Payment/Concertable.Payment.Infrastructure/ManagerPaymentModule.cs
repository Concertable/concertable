using Concertable.User.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Payment.Infrastructure;

internal class ManagerPaymentModule : IManagerPaymentModule
{
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly IUserModule userModule;

    public ManagerPaymentModule(
        IPaymentService paymentService,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository,
        IUserModule userModule)
    {
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.userModule = userModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string paymentMethodId,
        CancellationToken ct = default)
    {
        var payer = await userModule.GetManagerByIdAsync(payerId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerId}");
        var payee = await userModule.GetManagerByIdAsync(payeeId)
            ?? throw new NotFoundException($"Payee manager not found for userId {payeeId}");

        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(payerId)
            ?? throw new NotFoundException($"Payout account not found for payer {payerId}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payeeId)
            ?? throw new NotFoundException($"Payout account not found for payee {payeeId}");

        var payeeStripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has not completed Stripe verification");

        if (await stripeAccountService.GetAccountStatusAsync(payeeStripeAccountId) != PayoutAccountStatus.Verified)
            throw new BadRequestException("Payee has not completed Stripe verification");

        var mergedMetadata = new Dictionary<string, string>
        {
            ["fromUserId"] = payerId.ToString(),
            ["fromUserEmail"] = payer.Email ?? string.Empty,
            ["toUserId"] = payeeId.ToString(),
            ["amount"] = ((long)(amount * 100)).ToString()
        }
        .Merge(metadata);

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = payer.Email ?? string.Empty,
            StripeCustomerId = payerAccount.StripeCustomerId,
            DestinationStripeId = payeeStripeAccountId,
            Amount = amount,
            Metadata = mergedMetadata
        });
    }

    public async Task<bool> HasStripeCustomerAsync(Guid userId)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId);
        return account?.StripeCustomerId is not null;
    }

    public async Task<string?> TryGetPaymentMethodIdAsync(Guid userId)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId);
        return await stripeAccountService.TryGetPaymentMethodAsync(account?.StripeCustomerId);
    }

    public async Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountService.CreatePaymentSessionAsync(stripeCustomerId, metadata, ct);
    }

    public async Task<CheckoutSession> CreateSavedCardSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountService.CreateSavedCardSessionAsync(stripeCustomerId, metadata, ct);
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
