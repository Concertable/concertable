using Concertable.Payment.Application.Interfaces.Webhook;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Concertable.Payment.Api.Controllers;

[Route("api/[controller]")]
internal class WebhookController : ControllerBase
{
    private readonly IWebhookService webhookService;

    public WebhookController(IWebhookService webhookService)
    {
        this.webhookService = webhookService;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            await webhookService.HandleAsync(json, Request.Headers["Stripe-Signature"]!);
        }
        catch (StripeException)
        {
            return Problem("Webhook validation failed");
        }

        return Ok();
    }
}
