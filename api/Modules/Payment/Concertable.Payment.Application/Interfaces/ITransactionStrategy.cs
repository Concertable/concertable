using Concertable.Payment.Contracts.Events;

namespace Concertable.Payment.Application.Interfaces;

internal interface ITransactionStrategy
{
    Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct);
}
