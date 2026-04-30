using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Services;

internal sealed class PaymentManager : IPaymentManager
{
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly IStripePaymentIntentClientFactory intentClientFactory;
    private readonly ILogger<PaymentManager> logger;

    public PaymentManager(
        IPayoutAccountRepository payoutAccountRepository,
        IStripePaymentIntentClientFactory intentClientFactory,
        ILogger<PaymentManager> logger)
    {
        this.payoutAccountRepository = payoutAccountRepository;
        this.intentClientFactory = intentClientFactory;
        this.logger = logger;
    }

    public async Task<Result<PaymentResponse>> ChargeAsync(ChargeRequest r, CancellationToken ct = default)
    {
        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(r.PayerId, ct)
            ?? throw new NotFoundException($"Payout account not found for payer {r.PayerId}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(r.PayeeId, ct)
            ?? throw new NotFoundException($"Payout account not found for payee {r.PayeeId}");

        var stripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var destinationStripeId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has no Stripe Connect account");

        var metadata = new Dictionary<string, string>
        {
            ["fromUserId"] = r.PayerId.ToString(),
            ["fromUserEmail"] = r.PayerEmail,
            ["toUserId"] = r.PayeeId.ToString(),
            ["amount"] = ((long)(r.Amount * 100)).ToString()
        }
        .Merge(r.Metadata);

        logger.LogInformation(
            "Charging {PayerId} {Amount} GBP -> {PayeeId} (stripe {DestinationStripeId}) for {Purpose}",
            r.PayerId, r.Amount, r.PayeeId, destinationStripeId, r.Metadata["type"]);

        return await intentClientFactory.Create(r.Session).ChargeAsync(new StripeChargeOptions
        {
            Amount = r.Amount,
            PaymentMethodId = r.PaymentMethodId,
            StripeCustomerId = stripeCustomerId,
            DestinationStripeId = destinationStripeId,
            ReceiptEmail = r.PayerEmail,
            Metadata = metadata
        });
    }
}
