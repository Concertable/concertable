using Concertable.Messaging.Contracts;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.Payment.Infrastructure;
using Concertable.Payment.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services.Webhook;

internal sealed class WebhookProcessor : IWebhookProcessor
{
    private readonly PaymentDbContext context;
    private readonly IStripeEventRepository stripeEventRepository;
    private readonly IBus integrationEventBus;
    private readonly IDbContextAccessor contextAccessor;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<WebhookProcessor> logger;

    public WebhookProcessor(
        PaymentDbContext context,
        IStripeEventRepository stripeEventRepository,
        IBus integrationEventBus,
        IDbContextAccessor contextAccessor,
        TimeProvider timeProvider,
        ILogger<WebhookProcessor> logger)
    {
        this.context = context;
        this.stripeEventRepository = stripeEventRepository;
        this.integrationEventBus = integrationEventBus;
        this.contextAccessor = contextAccessor;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task ProcessAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        try
        {
            logger.ProcessingStripeEvent(stripeEvent.Id, stripeEvent.Type);

            var dataObject = stripeEvent.Data.Object;
            if (dataObject is not (PaymentIntent or SetupIntent))
            {
                logger.SkippingStripeEventUnhandledObject(stripeEvent.Id, dataObject?.GetType().Name ?? "null");
                return;
            }

            if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
            {
                logger.SkippingStripeEventAlreadyProcessed(stripeEvent.Id);
                return;
            }

            stripeEventRepository.AddEvent(StripeEventEntity.Create(stripeEvent.Id, timeProvider.GetUtcNow().DateTime));
            contextAccessor.Context = context;

            switch (dataObject)
            {
                case PaymentIntent intent:
                    await ProcessPaymentIntentAsync(stripeEvent, intent, cancellationToken);
                    break;

                case SetupIntent intent:
                    await ProcessSetupIntentAsync(stripeEvent, intent, cancellationToken);
                    break;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.StripeWebhookProcessingError(stripeEvent.Id, ex);
            throw;
        }
        finally
        {
            contextAccessor.Context = null;
        }
    }

    private async Task ProcessPaymentIntentAsync(
        Event stripeEvent,
        PaymentIntent intent,
        CancellationToken cancellationToken)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                logger.PublishingPaymentSucceededEvent(intent.Id, stripeEvent.Id, intent.Metadata.GetValueOrDefault(PaymentMetadataKeys.Type, "unknown"));
                await integrationEventBus.PublishAsync(new PaymentSucceededEvent(intent.Id, intent.Metadata), cancellationToken);
                break;

            case EventTypes.PaymentIntentPaymentFailed:
                var error = intent.LastPaymentError;
                logger.PublishingPaymentFailedEvent(intent.Id, stripeEvent.Id, intent.Metadata.GetValueOrDefault(PaymentMetadataKeys.Type, "unknown"), error?.Code, error?.Message);
                await integrationEventBus.PublishAsync(new PaymentFailedEvent(intent.Id, error?.Code, error?.Message, intent.Metadata), cancellationToken);
                break;

            default:
                logger.SkippingStripeEventNotHandled(stripeEvent.Id, stripeEvent.Type);
                break;
        }
    }

    private async Task ProcessSetupIntentAsync(
        Event stripeEvent,
        SetupIntent intent,
        CancellationToken cancellationToken)
    {
        if (stripeEvent.Type is not (EventTypes.SetupIntentSucceeded or EventTypes.SetupIntentSetupFailed))
        {
            logger.SkippingStripeEventNotHandled(stripeEvent.Id, stripeEvent.Type);
            return;
        }

        if (intent.Metadata.GetValueOrDefault(PaymentMetadataKeys.Type) != TransactionTypes.Verify)
            return;

        switch (stripeEvent.Type)
        {
            case EventTypes.SetupIntentSucceeded:
                var enrichedMetadata = new Dictionary<string, string>(intent.Metadata)
                {
                    [PaymentMetadataKeys.PaymentMethodId] = intent.PaymentMethodId
                };
                logger.PublishingVerifyPaymentSucceededEvent(intent.Id, stripeEvent.Id);
                await integrationEventBus.PublishAsync(new PaymentSucceededEvent(intent.Id, enrichedMetadata), cancellationToken);
                break;

            case EventTypes.SetupIntentSetupFailed:
                var error = intent.LastSetupError;
                logger.PublishingVerifyPaymentFailedEvent(intent.Id, stripeEvent.Id, error?.Code, error?.Message);
                await integrationEventBus.PublishAsync(new PaymentFailedEvent(intent.Id, error?.Code, error?.Message, intent.Metadata), cancellationToken);
                break;
        }
    }
}
