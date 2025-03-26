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

        [HttpPost("add-bank-account")]
        public async Task<ActionResult<AddedBankAccountResponse>> AddBankAccountForUser([FromQuery]string token)
        {
            var user = await currentUserService.GetEntityAsync();

            if(user.StripeId is null)
            {
                return BadRequest("No StripeId found");
            }

            return Ok(await stripeAccountService.AddBankAccountAsync(user.StripeId, token));
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
