using Concertable.Payment.Contracts.Events;

namespace Concertable.Payment.Infrastructure.Events;

internal class SettlementTransactionService : ITransactionStrategy
{
    private readonly ITransactionService transactionService;

    public SettlementTransactionService(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
        => await transactionService.CompleteAsync(@event.TransactionId);
}
