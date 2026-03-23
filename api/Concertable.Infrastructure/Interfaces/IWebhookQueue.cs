using Stripe;

namespace Infrastructure.Interfaces;

public interface IWebhookQueue
{
    Task EnqueueAsync(Event stripeEvent);
}
