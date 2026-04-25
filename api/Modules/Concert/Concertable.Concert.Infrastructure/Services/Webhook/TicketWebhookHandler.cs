using Concertable.Payment.Application.Interfaces;
using Concertable.Core.Enums;
using Concertable.Payment.Application.Interfaces.Webhook;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Concert.Infrastructure.Services.Webhook;

internal class TicketWebhookHandler : ITicketWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ITicketService ticketService;
    private readonly ITicketNotificationService notificationService;
    private readonly ILogger<TicketWebhookHandler> logger;
    private readonly TimeProvider timeProvider;

    public TicketWebhookHandler(
        ITransactionService transactionService,
        ITicketService ticketService,
        ITicketNotificationService notificationService,
        ILogger<TicketWebhookHandler> logger,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.ticketService = ticketService;
        this.notificationService = notificationService;
        this.logger = logger;
        this.timeProvider = timeProvider;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        var fromUserId = Guid.Parse(intent.Metadata["fromUserId"]);
        var toUserId = Guid.Parse(intent.Metadata["toUserId"]);
        var concertId = int.Parse(intent.Metadata["concertId"]);

        await transactionService.LogAsync(new TicketTransactionDto
        {
            ConcertId = concertId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            PaymentIntentId = intent.Id,
            Amount = intent.AmountReceived,
            Status = TransactionStatus.Complete,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });

        var result = await ticketService.CompleteAsync(new PurchaseCompleteDto
        {
            TransactionId = intent.Id,
            FromUserId = fromUserId,
            FromEmail = intent.Metadata["fromUserEmail"],
            ToUserId = toUserId,
            EntityId = concertId,
            Quantity = int.Parse(intent.Metadata["quantity"])
        });

        if (result.IsFailed)
        {
            logger.LogError("Failed to complete ticket purchase for concert {ConcertId}: {Errors}", concertId, result.Errors);
            return;
        }

        await notificationService.TicketPurchasedAsync(fromUserId.ToString(), result.Value);
    }
}
