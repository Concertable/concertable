using Concertable.Shared.Exceptions;

namespace Concertable.Payment.Infrastructure.Services;

internal class ManagerPaymentService : IManagerPaymentService
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly ITransactionService transactionService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly TimeProvider timeProvider;

    public ManagerPaymentService(
        IStripeAccountService stripeAccountService,
        IPaymentService paymentService,
        ITransactionService transactionService,
        IPayoutAccountRepository payoutAccountRepository,
        TimeProvider timeProvider)
    {
        this.stripeAccountService = stripeAccountService;
        this.paymentService = paymentService;
        this.transactionService = transactionService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<PaymentResponse> PayAsync(ManagerDto payer, ManagerDto payee, decimal amount, int bookingId, string? paymentMethodId = null)
    {
        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(payer.Id)
            ?? throw new NotFoundException($"Stripe account not found for payer {payer.Id}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payee.Id)
            ?? throw new NotFoundException($"Stripe account not found for payee {payee.Id}");

        var payerStripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var payeeStripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has not completed Stripe verification");

        if (await stripeAccountService.GetAccountStatusAsync(payeeStripeAccountId) != PayoutAccountStatus.Verified)
            throw new BadRequestException("Payee has not completed Stripe verification");

        paymentMethodId ??= await stripeAccountService.GetPaymentMethodAsync(payerStripeCustomerId);

        var response = await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = payer.Email ?? string.Empty,
            StripeCustomerId = payerStripeCustomerId,
            Amount = amount,
            DestinationStripeId = payeeStripeAccountId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", payer.Id.ToString() },
                { "toUserId", payee.Id.ToString() },
                { "type", "settlement" },
                { "bookingId", bookingId.ToString() }
            }
        });

        if (response.IsFailed)
            throw new InternalServerException(string.Join(", ", response.Errors.Select(e => e.Message)));

        if (response.Value.TransactionId is null)
            throw new InternalServerException("Payment did not return a valid PaymentIntent ID");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            BookingId = bookingId,
            FromUserId = payer.Id,
            ToUserId = payee.Id,
            PaymentIntentId = response.Value.TransactionId,
            Amount = (long)(amount * 100),
            Status = TransactionStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });

        return response.Value;
    }
}
