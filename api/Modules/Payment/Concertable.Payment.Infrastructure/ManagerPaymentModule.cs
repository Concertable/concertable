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
    private readonly IEscrowRepository escrowRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUserModule userModule;
    private readonly TimeProvider timeProvider;

    public ManagerPaymentModule(
        IPaymentManager paymentManager,
        IStripeAccountClient stripeAccountClient,
        IPayoutAccountRepository payoutAccountRepository,
        IEscrowRepository escrowRepository,
        ITransactionRepository transactionRepository,
        IUserModule userModule,
        TimeProvider timeProvider)
    {
        this.paymentManager = paymentManager;
        this.stripeAccountClient = stripeAccountClient;
        this.payoutAccountRepository = payoutAccountRepository;
        this.escrowRepository = escrowRepository;
        this.transactionRepository = transactionRepository;
        this.userModule = userModule;
        this.timeProvider = timeProvider;
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

    public async Task<Result<EscrowResponse>> HoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        DateTime releaseAt,
        CancellationToken ct = default)
    {
        var payer = await userModule.GetManagerByIdAsync(payerId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerId}");

        if (session == PaymentSession.OffSession && !await HasStripeCustomerAsync(payerId, ct))
            throw new BadRequestException("Stripe customer setup is required for off-session payments.");

        var holdMetadata = new Dictionary<string, string>(metadata) { ["type"] = TransactionTypes.Escrow };

        var hold = await paymentManager.HoldAsync(new HoldRequest
        {
            PayerId = payerId,
            PayerEmail = payer.Email ?? string.Empty,
            PayeeId = payeeId,
            Amount = amount,
            PaymentMethodId = paymentMethodId,
            Metadata = holdMetadata,
            Session = session
        }, ct);

        if (hold.IsFailed)
            return hold.ToResult<EscrowResponse>();

        if (string.IsNullOrEmpty(hold.Value.TransactionId))
            return Result.Fail("Stripe hold response missing PaymentIntent id.");

        var escrow = EscrowEntity.Create(
            bookingId,
            payerId,
            payeeId,
            (long)(amount * 100),
            hold.Value.TransactionId,
            releaseAt);

        await escrowRepository.AddAsync(escrow);
        await escrowRepository.SaveChangesAsync();

        if (!hold.Value.RequiresAction)
        {
            escrow.Confirm();
            await escrowRepository.SaveChangesAsync();
        }

        return Result.Ok(new EscrowResponse(escrow.Id, escrow.ChargeId, escrow.Status, hold.Value.ClientSecret));
    }

    public async Task<Result<TransferResponse>> ReleaseAsync(int escrowId, CancellationToken ct = default)
    {
        var escrow = await escrowRepository.GetByIdAsync(escrowId)
            ?? throw new NotFoundException($"Escrow {escrowId} not found");

        if (escrow.Status != EscrowStatus.Held)
            return Result.Fail($"Escrow {escrowId} is {escrow.Status}, not Held");

        var release = await paymentManager.ReleaseAsync(new ReleaseRequest
        {
            PayeeId = escrow.ToUserId,
            Amount = escrow.Amount / 100m,
            ChargeId = escrow.ChargeId,
            Metadata = new Dictionary<string, string>
            {
                ["type"] = "escrowRelease",
                ["escrowId"] = escrow.Id.ToString(),
                ["bookingId"] = escrow.BookingId.ToString()
            }
        }, ct);

        if (release.IsFailed)
            return release;

        escrow.Release(release.Value.TransferId, timeProvider.GetUtcNow().DateTime);
        await escrowRepository.SaveChangesAsync();

        return release;
    }

    public async Task<Result<RefundResponse>> RefundAsync(
        int escrowId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var escrow = await escrowRepository.GetByIdAsync(escrowId)
            ?? throw new NotFoundException($"Escrow {escrowId} not found");

        if (escrow.Status is not (EscrowStatus.Held or EscrowStatus.Released or EscrowStatus.Disputed))
            return Result.Fail($"Escrow {escrowId} is {escrow.Status}; cannot refund");

        var refundAmount = amount ?? escrow.Amount / 100m;

        var refund = await paymentManager.RefundAsync(new RefundRequest
        {
            Amount = refundAmount,
            PaymentIntentId = escrow.ChargeId,
            TransferId = escrow.TransferId,
            Reason = reason,
            Metadata = new Dictionary<string, string>
            {
                ["type"] = "escrowRefund",
                ["escrowId"] = escrow.Id.ToString(),
                ["bookingId"] = escrow.BookingId.ToString()
            }
        }, ct);

        if (refund.IsFailed)
            return refund;

        escrow.Refund(refund.Value.RefundId, timeProvider.GetUtcNow().DateTime);
        await escrowRepository.SaveChangesAsync();

        return refund;
    }

    public async Task<EscrowDto?> GetEscrowByBookingIdAsync(int bookingId, CancellationToken ct = default)
    {
        var escrow = await escrowRepository.GetByBookingIdAsync(bookingId, ct);
        if (escrow is null)
            return null;

        return new EscrowDto(
            escrow.Id,
            escrow.BookingId,
            escrow.FromUserId,
            escrow.ToUserId,
            escrow.Amount / 100m,
            escrow.Status,
            escrow.ChargeId,
            escrow.TransferId,
            escrow.RefundId,
            escrow.ReleaseAt,
            escrow.ReleasedAt,
            escrow.RefundedAt);
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
