using Application.Interfaces;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeAccountController : ControllerBase
    {
        private readonly IStripeAccountService stripeAccountService;
        private readonly ICurrentUserService currentUserService;

        public StripeAccountController(IStripeAccountService stripeAccountService, ICurrentUserService currentUserService)
        {
            this.stripeAccountService = stripeAccountService;
            this.currentUserService = currentUserService;
        }

        [HttpGet("onboarding-link")]
        public async Task<ActionResult<string>> GetOnboardingLink()
        {
            var user = await currentUserService.GetEntityAsync();

            if (string.IsNullOrWhiteSpace(user.StripeId))
                return BadRequest("User does not have a Stripe account set up");

            var link = await stripeAccountService.GetOnboardingLinkAsync(user.StripeId);
            return Ok(link);
        }


        [HttpGet("verified")]
        public async Task<ActionResult<bool>> IsUserVerified()
        {
            var user = await currentUserService.GetEntityAsync();

            if (user.StripeId is null)
            {
                return NotFound(false);
            }

            return Ok(await stripeAccountService.IsUserVerifiedAsync(user.StripeId));   
        }
    }
}
