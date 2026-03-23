using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Webhook;

public class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementProcessor settlementProcessor;
    private readonly TimeProvider timeProvider;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        ISettlementProcessor settlementProcessor,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.settlementProcessor = settlementProcessor;
        this.timeProvider = timeProvider;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        var fromUserId = int.Parse(intent.Metadata["fromUserId"]);
        var toUserId = int.Parse(intent.Metadata["toUserId"]);

        await transactionService.LogAsync(new TransactionDto
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            TransactionId = intent.Id,
            Amount = intent.AmountReceived,
            Type = intent.Metadata["type"],
            Status = intent.Status,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });

        await settlementProcessor.SettleAsync(int.Parse(intent.Metadata["applicationId"]));
    }
}
