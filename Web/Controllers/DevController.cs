using Application.Interfaces;
using Infrastructure.Data.Identity;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using Web.Hubs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevController : ControllerBase
    {
        private readonly StripeSettings stripeSettings;
        private readonly IHubContext<PaymentHub> hubContext;
        private readonly IEmailService emailService;

        public DevController(
            IOptions<StripeSettings> stripeOptions,
            IHubContext<PaymentHub> hubContext,
            IEmailService emailService)
        {
            stripeSettings = stripeOptions.Value;
            this.hubContext = hubContext;
            this.emailService = emailService;
        }

        [HttpGet("di-stripe-key")]
        public IActionResult GetStripeKeyFromDI()
        {
            return Ok(new
            {
                FromStripeSettings = stripeSettings.SecretKey?.Substring(0, 8) ?? "NULL"
            });
        }

        [HttpPost("signalr-test")]
        public async Task<IActionResult> SendTestSignalR([FromQuery] string userId, [FromBody] object testPayload)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Missing userId");

            await hubContext.Clients.Group(userId).SendAsync("EventCreated", testPayload);

            return Ok(new
            {
                SentToGroup = userId,
                Payload = testPayload
            });
        }

        [HttpPost("broadcast-test")]
        public async Task<IActionResult> BroadcastTestEvent([FromBody] object testPayload)
        {
            await hubContext.Clients.All.SendAsync("EventCreated", testPayload);

            return Ok(new
            {
                SentTo = "Everyone",
                Payload = testPayload
            });
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                return BadRequest("Missing query param: ?to=someone@example.com");

            try
            {
                await emailService.SendEmailAsync(
                    userId: 0,
                    toEmail: to,
                    subject: "Test Email from Concertable DevController",
                    body: "<p>This is a <strong>test email</strong> from your API in development mode.</p>"
                );

                return Ok(new { Message = $"Test email sent to {to}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to send test email",
                    ex.Message
                });
            }
        }

        [HttpGet("debug-db-name")]
        public IActionResult GetActualDatabaseName([FromServices] ApplicationDbContext dbContext)
        {
            var dbName = dbContext.Database.GetDbConnection().Database;
            var dbServer = dbContext.Database.GetDbConnection().DataSource;

            return Ok(new
            {
                Server = dbServer,
                Database = dbName
            });
        }



    }
}
