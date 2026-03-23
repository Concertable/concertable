using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Web.Controllers;

[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookQueue webhookQueue;
    private readonly string webhookSecret;

    public WebhookController(IWebhookQueue webhookQueue, IConfiguration configuration)
    {
        this.webhookQueue = webhookQueue;
        webhookSecret = configuration["Stripe:WebhookSecret"]!;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
            await webhookQueue.EnqueueAsync(stripeEvent);
        }
        catch (StripeException)
        {
            return Problem("Webhook validation failed");
        }

        return Ok();
    }
}
