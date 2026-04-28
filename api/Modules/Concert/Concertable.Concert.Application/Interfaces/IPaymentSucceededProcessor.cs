using Concertable.Payment.Contracts.Events;

namespace Concertable.Concert.Application.Interfaces;

internal interface IPaymentSucceededProcessor
{
    Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct);
}
