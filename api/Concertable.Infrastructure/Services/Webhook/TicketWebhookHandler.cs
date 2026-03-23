using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Payment;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Webhook;

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
        var fromUserId = int.Parse(intent.Metadata["fromUserId"]);
        var toUserId = int.Parse(intent.Metadata["toUserId"]);
        var concertId = int.Parse(intent.Metadata["concertId"]);

        await transactionService.LogAsync(new TicketTransactionDto
        {
            ConcertId = concertId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            TransactionId = intent.Id,
            Amount = intent.AmountReceived,
            Status = intent.Status,
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
