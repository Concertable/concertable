using Stripe;

namespace Concertable.Infrastructure.Interfaces;

public interface IWebhookQueue
{
    Task EnqueueAsync(Event stripeEvent);
}
