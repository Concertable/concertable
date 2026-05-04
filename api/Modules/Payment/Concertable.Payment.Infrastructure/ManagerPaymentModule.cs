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
    private readonly IStripeAccountClient stripeAccountClient;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUserModule userModule;

    public ManagerPaymentModule(
        IPaymentManager paymentManager,
        IStripeAccountClient stripeAccountClient,
        IPayoutAccountRepository payoutAccountRepository,
        ITransactionRepository transactionRepository,
        IUserModule userModule)
    {
        this.paymentManager = paymentManager;
        this.stripeAccountClient = stripeAccountClient;
        this.payoutAccountRepository = payoutAccountRepository;
        this.transactionRepository = transactionRepository;
        this.userModule = userModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        CancellationToken ct = default)
    {
        var payer = await userModule.GetManagerByIdAsync(payerId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerId}");

        if (session == PaymentSession.OffSession && !await HasStripeCustomerAsync(payerId, ct))
            throw new BadRequestException("Stripe customer setup is required for off-session payments.");

        var charge = await paymentManager.ChargeAsync(new ChargeRequest
        {
            PayerId = payerId,
            PayerEmail = payer.Email ?? string.Empty,
            PayeeId = payeeId,
            Amount = amount,
            PaymentMethodId = paymentMethodId,
            Metadata = new Dictionary<string, string> { ["type"] = TransactionTypes.Settlement },
            Session = session
        }, ct);

        if (charge.IsFailed)
            return charge;

        if (string.IsNullOrEmpty(charge.Value.TransactionId))
            return Result.Fail("Stripe charge response missing PaymentIntent id.");

        var transaction = SettlementTransactionEntity.Create(
            payerId,
            payeeId,
            charge.Value.TransactionId,
            (long)(amount * 100),
            TransactionStatus.Pending,
            bookingId);

        await transactionRepository.CreateAsync(transaction);

        if (!charge.Value.RequiresAction)
        {
            transaction.Complete();
            await transactionRepository.SaveChangesAsync();
        }

        return charge;
    }

    public async Task<CheckoutSession> CreatePaymentSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountClient.CreatePaymentSessionAsync(stripeCustomerId, metadata, ct);
    }

    public async Task<CheckoutSession> CreateSetupSessionAsync(
        Guid payerId,
        IDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        return await stripeAccountClient.CreateSetupSessionAsync(stripeCustomerId, metadata, ct);
    }

    public async Task VerifyAndVoidAsync(Guid payerId, string paymentMethodId, CancellationToken ct = default)
    {
        var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
        await stripeAccountClient.VerifyAndVoidAsync(stripeCustomerId, paymentMethodId, ct);
    }

    private async Task<bool> HasStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId is not null;
    }

    private async Task<string> EnsureStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        if (account?.StripeCustomerId is not null)
            return account.StripeCustomerId;

        var manager = await userModule.GetManagerByIdAsync(userId)
            ?? throw new NotFoundException($"Manager not found for userId {userId}");

        await stripeAccountClient.ProvisionCustomerAsync(userId, manager.Email ?? string.Empty, ct);

        account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId
            ?? throw new InvalidOperationException("Failed to provision Stripe customer.");
    }
}
