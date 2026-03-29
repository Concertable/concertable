using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Interfaces;
using Stripe;

namespace Concertable.Infrastructure.Services.Webhook;

public class TicketWebhookHandler : ITicketWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ITicketService ticketService;
    private readonly ITicketNotificationService notificationService;
    private readonly TimeProvider timeProvider;

    public TicketWebhookHandler(
        ITransactionService transactionService,
        ITicketService ticketService,
        ITicketNotificationService notificationService,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.ticketService = ticketService;
        this.notificationService = notificationService;
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

        var response = await ticketService.CompleteAsync(new PurchaseCompleteDto
        {
            TransactionId = intent.Id,
            FromUserId = fromUserId,
            FromEmail = intent.Metadata["fromUserEmail"],
            ToUserId = toUserId,
            EntityId = concertId,
            Quantity = int.Parse(intent.Metadata["quantity"])
        });

        await notificationService.TicketPurchasedAsync(fromUserId.ToString(), response);
    }
}
