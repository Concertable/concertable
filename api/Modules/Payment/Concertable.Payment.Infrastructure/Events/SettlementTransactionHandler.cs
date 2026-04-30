using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Events;

internal class SettlementTransactionHandler : ITransactionHandler
{
    private readonly ITransactionService transactionService;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<SettlementTransactionHandler> logger;

    public SettlementTransactionHandler(ITransactionService transactionService, TimeProvider timeProvider, ILogger<SettlementTransactionHandler> logger)
    {
        this.transactionService = transactionService;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var meta = @event.Metadata;
        var bookingId = int.Parse(meta["bookingId"]);
        var amount = long.Parse(meta["amount"]);

        logger.LogInformation(
            "Recording settlement transaction {TransactionId} for booking {BookingId}: {Amount} pence {Currency}",
            @event.TransactionId, bookingId, amount, "GBP");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            BookingId = bookingId,
            FromUserId = Guid.Parse(meta["fromUserId"]),
            ToUserId = Guid.Parse(meta["toUserId"]),
            PaymentIntentId = @event.TransactionId,
            Amount = amount,
            Status = TransactionStatus.Complete,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });
    }
}
