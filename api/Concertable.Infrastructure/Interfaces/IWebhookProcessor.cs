using Stripe;

namespace Infrastructure.Interfaces;

public interface IWebhookProcessor
{
    Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken);
}
