using Concertable.Payment.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Events;

internal class SettlementTransactionHandler : ITransactionHandler
{
    private readonly ITransactionRepository transactionRepository;
    private readonly ILogger<SettlementTransactionHandler> logger;

    public SettlementTransactionHandler(
        ITransactionRepository transactionRepository,
        ILogger<SettlementTransactionHandler> logger)
    {
        this.transactionRepository = transactionRepository;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
        var transaction = await transactionRepository.GetByPaymentIntentIdAsync(@event.TransactionId);
        if (transaction is null)
        {
            logger.LogWarning(
                "No settlement transaction found for charge {ChargeId}; ignoring PaymentSucceededEvent",
                @event.TransactionId);
            return;
        }

        if (transaction.Status != TransactionStatus.Pending)
        {
            logger.LogInformation(
                "Settlement transaction {TransactionId} already in status {Status}; skipping complete",
                transaction.Id, transaction.Status);
            return;
        }

        transaction.Complete();
        await transactionRepository.SaveChangesAsync();

        logger.LogInformation(
            "Settlement transaction {TransactionId} completed (Pending -> Complete) for charge {ChargeId}",
            transaction.Id, transaction.PaymentIntentId);
    }
}
