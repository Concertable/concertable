using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ITicketService ticketService;
        private readonly IEventService eventService;
        private readonly IPurchaseService purchaseService;
        private readonly IEmailService emailService;
        
        private readonly string webhookSecret;

        public WebhookController(
            ITicketService ticketService,
            IEventService eventService,
            IPurchaseService purchaseService,
            IEmailService emailService,
            IConfiguration configuration)
        {
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
                    bool processed = false;

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
                        await ticketService.CompleteAsync(purchaseCompleteDto);
                        processed = true;
                    }
                    else if (paymentType == "application")
                    {
                        purchaseCompleteDto.EntityId = int.Parse(intent.Metadata["applicationId"]);
                        await eventService.CompleteAsync(purchaseCompleteDto);
                        processed = true;
                    }
                    if (processed)
                    {
                        await purchaseService.LogAsync(purchaseDto);
                    }

                    return Ok();
            }
            return BadRequest();
        }
    }
}
