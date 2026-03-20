using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Interfaces.Payment;
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

namespace Web.Controllers;

[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IHubContext<ConcertHub> hubContext;
    private readonly IBackgroundTaskQueue taskQueue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IStripeEventRepository stripeEventRepository;
    private readonly ITicketService ticketService;
    private readonly IConcertService concertService;
    private readonly ITransactionService purchaseService;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<WebhookController> logger;

    private readonly string webhookSecret;

    public WebhookController(
        IStripeEventRepository stripeEventRepository,
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory,
        IHubContext<ConcertHub> hubContext,
        ITicketService ticketService,
        IConcertService concertService,
        ITransactionService purchaseService,
        IConfiguration configuration,
        TimeProvider timeProvider,
        ILogger<WebhookController> logger)
    {
        this.hubContext = hubContext;
        this.taskQueue = taskQueue;
        this.scopeFactory = scopeFactory;
        this.stripeEventRepository = stripeEventRepository;
        this.ticketService = ticketService;
        this.concertService = concertService;
        this.purchaseService = purchaseService;
        webhookSecret = configuration["Stripe:WebhookSecret"]!;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        Stripe.Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        }
        catch (StripeException)
        {
            return Problem("Webhook validation failed");
        }

        await taskQueue.EnqueueAsync(async cancellationToken =>
        {
            using var scope = scopeFactory.CreateScope();
            await ProcessWebhook(scope, stripeEvent, cancellationToken);
        });

        return Ok();
    }

    private async Task ProcessWebhook(IServiceScope scope, Stripe.Event stripeEvent, CancellationToken cancellationToken)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebhookController>>();

        var stripeEventRepository = scope.ServiceProvider.GetRequiredService<IStripeEventRepository>();
        var purchaseService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
        var concertService = scope.ServiceProvider.GetRequiredService<IConcertService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ConcertHub>>();

        try
        {
            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return;

            if (await stripeEventRepository.EventExistsAsync(stripeEvent.Id))
                return;

            await stripeEventRepository.AddEventAsync(new StripeEventEntity
            {
                EventId = stripeEvent.Id,
                EventProcessedAt = timeProvider.GetUtcNow().DateTime
            });

            if (intent.Status == "succeeded")
            {
                var type = intent.Metadata["type"];
                var fromUserId = int.Parse(intent.Metadata["fromUserId"]);
                var toUserId = int.Parse(intent.Metadata["toUserId"]);
                var fromUserEmail = intent.Metadata["fromUserEmail"];

                var purchaseDto = new TransactionDto
                {
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    TransactionId = intent.Id,
                    Amount = intent.AmountReceived,
                    Type = type,
                    Status = intent.Status,
                    CreatedAt = timeProvider.GetUtcNow().DateTime
                };

                await purchaseService.LogAsync(purchaseDto);

                var purchaseCompleteDto = new PurchaseCompleteDto
                {
                    TransactionId = intent.Id,
                    FromUserId = fromUserId,
                    FromEmail = fromUserEmail,
                    ToUserId = toUserId,
                };

                if (type == "concert")
                {
                    purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["concertId"]);
                    purchaseCompleteDto.Quantity = int.Parse(intent.Metadata["quantity"]);
                    var response = await ticketService.CompleteAsync(purchaseCompleteDto);
                    await hubContext.Clients.Group(fromUserId.ToString()).SendAsync("TicketPurchased", response);
                }
                else if (type == "application")
                {
                    purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["applicationId"]);
                    var response = await concertService.CompleteAsync(purchaseCompleteDto);
                    await hubContext.Clients.Group(fromUserId.ToString()).SendAsync("ConcertCreated", response);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook for event {EventId}", stripeEvent.Id);
        }
    }
}
