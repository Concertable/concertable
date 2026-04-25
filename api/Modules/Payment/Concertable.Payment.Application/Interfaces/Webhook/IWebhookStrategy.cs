using Stripe;

namespace Concertable.Payment.Application.Interfaces.Webhook;

internal interface IWebhookStrategy
{
    Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
