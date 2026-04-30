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
        var amountPence = long.TryParse(meta.GetValueOrDefault("amount"), out var a) ? a : 0;

        logger.LogInformation(
            "Recording settlement transaction {TransactionId} for booking {BookingId}: {AmountPence} pence {Currency}",
            @event.TransactionId, bookingId, amountPence, "GBP");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            BookingId = bookingId,
            FromUserId = Guid.Parse(meta["fromUserId"]),
            ToUserId = Guid.Parse(meta["toUserId"]),
            PaymentIntentId = @event.TransactionId,
            Amount = amountPence,
            Status = TransactionStatus.Complete,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });
    }
}
