using Concertable.Payment.Contracts.Events;

namespace Concertable.Payment.Infrastructure.Events;

internal class SettlementTransactionHandler : ITransactionHandler
{
    private readonly ITransactionService transactionService;

    public SettlementTransactionHandler(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
        => await transactionService.CompleteAsync(@event.TransactionId);
}
