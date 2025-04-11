using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Web.Hubs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevController : ControllerBase
    {
        private readonly StripeSettings stripeSettings;
        private readonly IHubContext<PaymentHub> hubContext;

        public DevController(IOptions<StripeSettings> stripeOptions, IHubContext<PaymentHub> hubContext)
        {
            stripeSettings = stripeOptions.Value;
            this.hubContext = hubContext;
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

    }
}
