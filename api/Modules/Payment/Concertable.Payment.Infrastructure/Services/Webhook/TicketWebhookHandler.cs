using Concertable.Concert.Contracts;
using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Interfaces.Webhook;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services.Webhook;

internal class TicketWebhookHandler : ITicketWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ITicketPaymentModule ticketPaymentModule;
    private readonly ILogger<TicketWebhookHandler> logger;
    private readonly TimeProvider timeProvider;

    public TicketWebhookHandler(
        ITransactionService transactionService,
        ITicketPaymentModule ticketPaymentModule,
        ILogger<TicketWebhookHandler> logger,
        TimeProvider timeProvider)
    {
        this.transactionService = transactionService;
        this.ticketPaymentModule = ticketPaymentModule;
        this.logger = logger;
        this.timeProvider = timeProvider;
    }

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        var fromUserId = Guid.Parse(intent.Metadata["fromUserId"]);
        var toUserId = Guid.Parse(intent.Metadata["toUserId"]);
        var concertId = int.Parse(intent.Metadata["concertId"]);
        var quantity = int.Parse(intent.Metadata["quantity"]);

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

        var ticketIds = await ticketPaymentModule.IssueTicketsAsync(concertId, fromUserId, quantity, cancellationToken);

        if (ticketIds.Count == 0)
            logger.LogError("Failed to issue tickets for concert {ConcertId} (userId={UserId})", concertId, fromUserId);
    }
}
