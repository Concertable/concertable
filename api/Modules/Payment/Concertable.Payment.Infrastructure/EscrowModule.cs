using Concertable.User.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure;

internal class EscrowModule : IEscrowModule
{
    private readonly IPaymentManager paymentManager;
    private readonly IEscrowRepository escrowRepository;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly IUserModule userModule;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<EscrowModule> logger;

    public EscrowModule(
        IPaymentManager paymentManager,
        IEscrowRepository escrowRepository,
        IPayoutAccountRepository payoutAccountRepository,
        IUserModule userModule,
        TimeProvider timeProvider,
        ILogger<EscrowModule> logger)
    {
        this.paymentManager = paymentManager;
        this.escrowRepository = escrowRepository;
        this.payoutAccountRepository = payoutAccountRepository;
        this.userModule = userModule;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<Result<EscrowResponse>> HoldAsync(
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

        var hold = await paymentManager.HoldAsync(new HoldRequest
        {
            PayerId = payerId,
            PayerEmail = payer.Email ?? string.Empty,
            PayeeId = payeeId,
            Amount = amount,
            PaymentMethodId = paymentMethodId,
            Metadata = new Dictionary<string, string> { ["type"] = TransactionTypes.Escrow },
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
            hold.Value.TransactionId);

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

    public async Task<Result<TransferResponse?>> ReleaseByBookingIdAsync(int bookingId, CancellationToken ct = default)
    {
        var escrow = await escrowRepository.GetByBookingIdAsync(bookingId, ct);
        if (escrow is null)
        {
            logger.LogWarning("No escrow found for booking {BookingId}; nothing to release", bookingId);
            return Result.Ok<TransferResponse?>(null);
        }

        if (escrow.Status != EscrowStatus.Held)
        {
            logger.LogWarning(
                "Escrow {EscrowId} for booking {BookingId} is {Status}, not Held; skipping release",
                escrow.Id, bookingId, escrow.Status);
            return Result.Ok<TransferResponse?>(null);
        }

        var release = await ReleaseAsync(escrow.Id, ct);
        return release.IsFailed
            ? release.ToResult<TransferResponse?>()
            : Result.Ok<TransferResponse?>(release.Value);
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

    public async Task<EscrowDto?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
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
            escrow.ReleasedAt,
            escrow.RefundedAt);
    }

    private async Task<bool> HasStripeCustomerAsync(Guid userId, CancellationToken ct)
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(userId, ct);
        return account?.StripeCustomerId is not null;
    }
}
