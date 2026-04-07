using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
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
        var manager = currentUser.GetEntity<ManagerEntity>();

        if (string.IsNullOrWhiteSpace(manager.StripeAccountId))
            return BadRequest("You do not have a Stripe account, contact support to get one");

        var link = await stripeAccountService.GetOnboardingLinkAsync(manager.StripeAccountId);
        return Ok(link);
    }

    [HttpGet("verified")]
    public async Task<ActionResult<bool>> IsUserVerified()
    {
        var manager = currentUser.GetEntity<ManagerEntity>();

        if (manager.StripeAccountId is null)
            return NotFound(false);

        return Ok(await stripeAccountService.IsUserVerifiedAsync(manager.StripeAccountId));
    }
}
