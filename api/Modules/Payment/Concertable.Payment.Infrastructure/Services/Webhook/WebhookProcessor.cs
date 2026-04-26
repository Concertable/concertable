using Concertable.Payment.Contracts.Events;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services.Webhook;

internal class WebhookProcessor : IWebhookProcessor
{
    private readonly IStripeEventRepository stripeEventRepository;
    private readonly IIntegrationEventBus integrationEventBus;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<WebhookProcessor> logger;

    public WebhookProcessor(
        IStripeEventRepository stripeEventRepository,
        IIntegrationEventBus integrationEventBus,
        TimeProvider timeProvider,
        ILogger<WebhookProcessor> logger)
    {
        this.stripeEventRepository = stripeEventRepository;
        this.integrationEventBus = integrationEventBus;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return;

            if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
                return;

            await stripeEventRepository.AddEventAsync(StripeEventEntity.Create(stripeEvent.Id, timeProvider.GetUtcNow().DateTime));

            if (intent.Status != "succeeded")
                return;

            await integrationEventBus.PublishAsync(new PaymentSucceededEvent(intent.Id, intent.Metadata), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook for event {EventId}", stripeEvent.Id);
        }
    }
}
