using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevController : ControllerBase
    {
        private readonly StripeSettings stripeSettings;

        public DevController(IOptions<StripeSettings> stripeOptions)
        {
            stripeSettings = stripeOptions.Value;
        }

        [HttpGet("di-stripe-key")]
        public IActionResult GetStripeKeyFromDI()
        {
            return Ok(new
            {
                FromStripeSettings = stripeSettings.SecretKey?.Substring(0, 8) ?? "NULL"
            });
        }
    }
}
