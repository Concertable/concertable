using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Azure;
using Core.Entities;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using StackExchange.Redis;
using Stripe;
using Stripe.V2;
using Web.Hubs;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<PaymentHub> hubContext;
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IStripeEventRepository stripeEventRepository;
        private readonly ITicketService ticketService;
        private readonly IEventService eventService;
        private readonly ITransactionService purchaseService;
        private readonly ILogger<WebhookController> logger;

        private readonly string webhookSecret;

        public WebhookController(
            IStripeEventRepository stripeEventRepository,
            IBackgroundTaskQueue taskQueue,
            IServiceScopeFactory scopeFactory,
            IHubContext<PaymentHub> hubContext,
            ITicketService ticketService,
            IEventService eventService,
            ITransactionService purchaseService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            this.hubContext = hubContext;
            this.taskQueue = taskQueue;
            this.scopeFactory = scopeFactory;
            this.stripeEventRepository = stripeEventRepository;
            this.ticketService = ticketService;
            this.eventService = eventService;
            this.purchaseService = purchaseService;
            webhookSecret = configuration["Stripe:WebhookSecret"];
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            logger.LogInformation("🔥 Webhook hit at {Time}", DateTime.UtcNow);
            logger.LogInformation("📦 Raw payload: {Json}", json);

            Stripe.Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
                logger.LogInformation("✅ Stripe event constructed: {EventId} | Type: {EventType}", stripeEvent.Id, stripeEvent.Type);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "❌ Webhook validation failed");
                return Problem("Webhook validation failed");
            }

            logger.LogInformation("📥 Enqueuing background work for event {EventId}", stripeEvent.Id);

            taskQueue.QueueBackgroundWorkItem(async cancellationToken =>
            {
                using var scope = scopeFactory.CreateScope();
                await ProcessStripeWebhook(scope, stripeEvent, cancellationToken);
            });

            logger.LogInformation("✅ Returning 200 OK to Stripe for event {EventId}", stripeEvent.Id);
            return Ok();
        }

        private async Task ProcessStripeWebhook(IServiceScope scope, Stripe.Event stripeEvent, CancellationToken cancellationToken)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebhookController>>();
            logger.LogInformation("⚙️ ProcessStripeWebhook invoked for event {EventId}", stripeEvent.Id);

            var stripeEventRepository = scope.ServiceProvider.GetRequiredService<IStripeEventRepository>();
            var purchaseService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PaymentHub>>();

            try
            {
                if (stripeEvent.Data.Object is not PaymentIntent intent)
                {
                    logger.LogWarning("🚫 Event is not a PaymentIntent. Skipping. EventId: {EventId}", stripeEvent.Id);
                    return;
                }

                logger.LogInformation("💳 PaymentIntent received: {IntentId} | Status: {Status}", intent.Id, intent.Status);

                if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
                {
                    logger.LogWarning("🛑 Duplicate event detected. Skipping processing. EventId: {EventId}", stripeEvent.Id);
                    return;
                }

                await stripeEventRepository.AddEventAsync(new StripeEvent
                {
                    EventId = stripeEvent.Id,
                    EventProcessedAt = DateTime.UtcNow
                });

                logger.LogInformation("🧾 Event recorded in database: {EventId}", stripeEvent.Id);

                if (intent.Status == "succeeded")
                {
                    var type = intent.Metadata["type"];
                    var fromUserId = int.Parse(intent.Metadata["fromUserId"]);
                    var toUserId = int.Parse(intent.Metadata["toUserId"]);
                    var fromUserEmail = intent.Metadata["fromUserEmail"];

                    logger.LogInformation("💸 Payment succeeded: Type={Type}, From={From}, To={To}", type, fromUserId, toUserId);

                    var purchaseDto = new TransactionDto
                    {
                        FromUserId = fromUserId,
                        ToUserId = toUserId,
                        TransactionId = intent.Id,
                        Amount = intent.AmountReceived,
                        Type = type,
                        Status = intent.Status,
                        CreatedAt = DateTime.Now
                    };

                    await purchaseService.LogAsync(purchaseDto);
                    logger.LogInformation("🧾 Purchase logged: {TransactionId}", intent.Id);

                    var purchaseCompleteDto = new PurchaseCompleteDto
                    {
                        TransactionId = intent.Id,
                        FromUserId = fromUserId,
                        FromEmail = fromUserEmail,
                        ToUserId = toUserId,
                    };

                    if (type == "event")
                    {
                        purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["eventId"]);
                        purchaseCompleteDto.Quantity = int.Parse(intent.Metadata["quantity"]);
                        var response = await ticketService.CompleteAsync(purchaseCompleteDto);
                        await hubContext.Clients.Group(fromUserId.ToString()).SendAsync("TicketPurchased", response);
                        logger.LogInformation("🎟️ Tickets completed for event: {EventId}", purchaseCompleteDto.EntityId);
                    }
                    else if (type == "application")
                    {
                        purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["applicationId"]);
                        var response = await eventService.CompleteAsync(purchaseCompleteDto);
                        await hubContext.Clients.Group(fromUserId.ToString()).SendAsync("EventCreated", response);
                        logger.LogInformation("📡 Event created and pushed to SignalR for user: {UserId}", fromUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "🔥 Error processing Stripe webhook for event {EventId}", stripeEvent.Id);
            }
        }

    }
}
