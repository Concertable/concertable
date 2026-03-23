using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
using Infrastructure.Data.Identity;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Infrastructure.Services.Webhook;

public class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementProcessor settlementProcessor;
    private readonly ApplicationDbContext context;
    private readonly TimeProvider timeProvider;

    public SettlementWebhookHandler(
        ITransactionService transactionService,
        ISettlementProcessor settlementProcessor,
        ApplicationDbContext context,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.settlementProcessor = settlementProcessor;
        this.context = context;
        this.timeProvider = timeProvider;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        var fromUserId = int.Parse(intent.Metadata["fromUserId"]);
        var toUserId = int.Parse(intent.Metadata["toUserId"]);
        var applicationId = int.Parse(intent.Metadata["applicationId"]);

        var concertId = await context.Concerts
            .Where(c => c.ApplicationId == applicationId)
            .Select(c => c.Id)
            .FirstAsync(cancellationToken);

        await transactionService.LogAsync(new ConcertTransactionDto
        {
            ConcertId = concertId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            TransactionId = intent.Id,
            Amount = intent.AmountReceived,
            Status = intent.Status,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });

        await settlementProcessor.SettleAsync(applicationId);
    }
}
