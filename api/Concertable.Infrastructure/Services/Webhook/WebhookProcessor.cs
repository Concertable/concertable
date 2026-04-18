using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Infrastructure.Services.Webhook;

public class WebhookProcessor : IWebhookProcessor
{
    private readonly IStripeEventRepository stripeEventRepository;
    private readonly IWebhookStrategyFactory strategyFactory;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<WebhookProcessor> logger;

    public WebhookProcessor(
        IStripeEventRepository stripeEventRepository,
        IWebhookStrategyFactory strategyFactory,
        TimeProvider timeProvider,
        ILogger<WebhookProcessor> logger)
    {
        this.stripeEventRepository = stripeEventRepository;
        this.strategyFactory = strategyFactory;
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

            var webhookType = Enum.Parse<WebhookType>(intent.Metadata["type"], ignoreCase: true);
            var strategy = strategyFactory.Create(webhookType);
            await strategy.HandleAsync(intent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook for event {EventId}", stripeEvent.Id);
        }
    }
}
