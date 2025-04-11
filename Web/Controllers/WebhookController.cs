using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
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
        private readonly IStripeEventRepository stripeEventRepository;
        private readonly ITicketService ticketService;
        private readonly IEventService eventService;
        private readonly IPurchaseService purchaseService;
        private readonly IEmailService emailService;
        private readonly ILogger<WebhookController> logger;

        private readonly string webhookSecret;

        public WebhookController(
            IStripeEventRepository stripeEventRepository,
            IHubContext<PaymentHub> hubContext,
            ITicketService ticketService,
            IEventService eventService,
            IPurchaseService purchaseService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            this.hubContext = hubContext;
            this.stripeEventRepository = stripeEventRepository;
            this.ticketService = ticketService;
            this.eventService = eventService;
            this.purchaseService = purchaseService;
            this.emailService = emailService;
            webhookSecret = configuration["Stripe:WebhookSecret"];
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            string json;
            Stripe.Event stripeEvent;

            try
            {
                json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Webhook validation failed");
                return Problem("Webhook validation failed");
            }

            // Respond to Stripe in the background
            _ = Task.Run(() => ProcessStripeWebhook(stripeEvent));

            return Ok(); // 200 OK immediately so stripe doesnt send another query
        }

        private async Task ProcessStripeWebhook(Stripe.Event stripeEvent)
        {
            try
            {
                if (stripeEvent.Data.Object is not PaymentIntent intent)
                    return;

                if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
                    return;

                var newEvent = new StripeEvent
                {
                    EventId = stripeEvent.Id,
                    EventProcessedAt = DateTime.UtcNow
                };

                await stripeEventRepository.AddEventAsync(newEvent);

                if (intent.Status == "succeeded")
                {
                    var paymentType = intent.Metadata["type"];
                    var purchaseDto = new PurchaseDto
                    {
                        FromUserId = int.Parse(intent.Metadata["fromUserId"]),
                        ToUserId = int.Parse(intent.Metadata["toUserId"]),
                        TransactionId = intent.Id,
                        Amount = intent.AmountReceived,
                        Type = paymentType,
                        Status = intent.Status,
                        CreatedAt = DateTime.Now
                    };

                    await purchaseService.LogAsync(purchaseDto);

                    var userId = int.Parse(intent.Metadata["fromUserId"]);

                    var purchaseCompleteDto = new PurchaseCompleteDto
                    {
                        TransactionId = intent.Id,
                        FromUserId = userId,
                        FromEmail = intent.Metadata["fromUserEmail"],
                        ToUserId = int.Parse(intent.Metadata["toUserId"]),
                    };

                    if (paymentType == "event")
                    {
                        purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["eventId"]);
                        purchaseCompleteDto.Quantity = int.Parse(intent.Metadata["quantity"]);
                        await ticketService.CompleteAsync(purchaseCompleteDto);
                    }
                    else if (paymentType == "application")
                    {
                        purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["applicationId"]);
                        var response = await eventService.CompleteAsync(purchaseCompleteDto);
                        await hubContext.Clients.Group(userId.ToString()).SendAsync("EventCreated", response);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe webhook in background");
            }
        }
    }
}
