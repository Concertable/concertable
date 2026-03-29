using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeAccountController : ControllerBase
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly ICurrentUser currentUser;

    public StripeAccountController(IStripeAccountService stripeAccountService, ICurrentUser currentUser)
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
    }

    [HttpGet("onboarding-link")]
    public async Task<ActionResult<string>> GetOnboardingLink()
    {
        var user = currentUser.GetEntity();

        if (string.IsNullOrWhiteSpace(user.StripeId))
            return BadRequest("You must have a Stripe Id, contact support to get one");

        var link = await stripeAccountService.GetOnboardingLinkAsync(user.StripeId);
        return Ok(link);
    }


    [HttpGet("verified")]
    public async Task<ActionResult<bool>> IsUserVerified()
    {
        var user = currentUser.GetEntity();

        if (user.StripeId is null)
        {
            return NotFound(false);
        }

        return Ok(await stripeAccountService.IsUserVerifiedAsync(user.StripeId));
    }
}
