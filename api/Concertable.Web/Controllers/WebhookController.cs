using Concertable.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Concertable.Web.Controllers;

[Route("api/[controller]")]
public class WebhookController : ControllerBase
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
