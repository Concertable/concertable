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
            logger.LogInformation(
                "Processing Stripe event {EventId} of type {EventType}",
                stripeEvent.Id, stripeEvent.Type);

            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                logger.LogInformation(
                    "Skipping Stripe event {EventId}: data object is {ObjectType}, not PaymentIntent",
                    stripeEvent.Id, stripeEvent.Data.Object?.GetType().Name ?? "null");
                return;
            }

            if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
            {
                logger.LogInformation(
                    "Skipping Stripe event {EventId}: already processed",
                    stripeEvent.Id);
                return;
            }

            await stripeEventRepository.AddEventAsync(StripeEventEntity.Create(stripeEvent.Id, timeProvider.GetUtcNow().DateTime));

            if (intent.Status != "succeeded")
            {
                logger.LogInformation(
                    "Skipping PaymentIntent {IntentId} (event {EventId}): status is {Status}, not succeeded",
                    intent.Id, stripeEvent.Id, intent.Status);
                return;
            }

            logger.LogInformation(
                "Publishing PaymentSucceededEvent for PaymentIntent {IntentId} (event {EventId}) of transaction type {TransactionType}",
                intent.Id, stripeEvent.Id, intent.Metadata["type"]);

            await integrationEventBus.PublishAsync(new PaymentSucceededEvent(intent.Id, intent.Metadata), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook for event {EventId}", stripeEvent.Id);
        }
    }
}
