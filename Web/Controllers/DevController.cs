using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevController : ControllerBase
    {
        [HttpGet("stripe-key")]
        public IActionResult GetStripeKey()
        {
            var rawEnv = Environment.GetEnvironmentVariable("Stripe__SecretKey");
            var configKey = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build()["Stripe:SecretKey"];

            return Ok(new
            {
                FromEnv = rawEnv?.Substring(0, 8) ?? "NULL",
                FromConfig = configKey?.Substring(0, 8) ?? "NULL"
            });
        }
    }
}
