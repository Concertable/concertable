using Stripe;

namespace Infrastructure.Interfaces;

public interface IWebhookStrategy
{
    Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken);
}
