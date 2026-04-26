using Concertable.Payment.Contracts.Events;

namespace Concertable.Concert.Application.Interfaces;

internal interface IPaymentSucceededStrategy
{
    Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct);
}
