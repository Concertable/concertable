using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;
using Microsoft.AspNetCore.Authorization;
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

        var link = await stripeAccountService.GetOnboardingLinkAsync(manager.StripeAccountId);
        return Ok(link);
    }

    [HttpGet("verified")]
    public async Task<ActionResult<bool>> IsUserVerified()
    {
        var manager = currentUser.GetEntity<ManagerEntity>();

        return Ok(await stripeAccountService.IsUserVerifiedAsync(manager.StripeAccountId));
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult<string>> CreateSetupIntent()
    {
        var user = currentUser.GetEntity<UserEntity>();

        if (string.IsNullOrWhiteSpace(user.StripeCustomerId))
            user.StripeCustomerId = await stripeAccountService.CreateCustomerAsync(user);

        return Ok(await stripeAccountService.CreateSetupIntentAsync(user.StripeCustomerId));
    }
}
