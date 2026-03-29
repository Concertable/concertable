using Stripe;

namespace Concertable.Infrastructure.Interfaces;

public interface IWebhookProcessor
{
    Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken);
}
