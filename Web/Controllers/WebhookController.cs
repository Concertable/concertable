using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Stripe;
using Web.Hubs;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<PaymentHub> hubContext;
        private readonly ITicketService ticketService;
        private readonly IEventService eventService;
        private readonly IPurchaseService purchaseService;
        private readonly IEmailService emailService;
        
        private readonly string webhookSecret;

        public WebhookController(
            IHubContext<PaymentHub> hubContext,
            ITicketService ticketService,
            IEventService eventService,
            IPurchaseService purchaseService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            this.hubContext = hubContext;
            this.ticketService = ticketService;
            this.eventService = eventService;
            this.purchaseService = purchaseService;
            this.emailService = emailService;
            webhookSecret = configuration["Stripe:WebhookSecret"];
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);

            if (stripeEvent.Data.Object is not PaymentIntent intent)
                return BadRequest("Invalid event data");

            switch (intent.Status)
            {
                case "succeeded":
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

                    /*
                     * Log the payment immediately
                     * in case we have any faults later in this controller (such as a database fail),
                     * the log is in the database so an administrator could issue a refund
                     */
                    await purchaseService.LogAsync(purchaseDto); 

                    var userId = int.Parse(intent.Metadata["fromUserId"]);
                    var email = intent.Metadata["fromUserEmail"];

                    var purchaseCompleteDto = new PurchaseCompleteDto
                    {
                        TransactionId = intent.Id,
                        FromUserId = int.Parse(intent.Metadata["fromUserId"]),
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
                        await hubContext.Clients.Group(userId.ToString()).SendAsync("ListingApplicationPurchaseResponse", response);
                    }

                    return Ok();
            }
            return BadRequest();
        }
    }
}
