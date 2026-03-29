using Stripe;

namespace Concertable.Infrastructure.Interfaces;

public interface IWebhookStrategy
{
    Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
