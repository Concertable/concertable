using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using Concertable.Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeAccountController : ControllerBase
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly ICurrentUser currentUser;
    private readonly IIdentityModule identityModule;
    private readonly ICurrentUserResolver currentUserResolver;

    public StripeAccountController(
        IStripeAccountService stripeAccountService,
        ICurrentUser currentUser,
        IIdentityModule identityModule,
        ICurrentUserResolver currentUserResolver)
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
        this.identityModule = identityModule;
        this.currentUserResolver = currentUserResolver;
    }

    [HttpGet("onboarding-link")]
    public async Task<ActionResult<string>> GetOnboardingLink()
    {
        var manager = await identityModule.GetManagerAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        var link = await stripeAccountService.GetOnboardingLinkAsync(manager.StripeAccountId);
        return Ok(link);
    }

    [HttpGet("account-status")]
    public async Task<ActionResult<PayoutAccountStatus>> GetAccountStatus()
    {
        var manager = await identityModule.GetManagerAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        return Ok(await stripeAccountService.GetAccountStatusAsync(manager.StripeAccountId));
    }

    [HttpGet("payment-method")]
    public async Task<ActionResult<PaymentMethodDto?>> GetPaymentMethod()
    {
        var user = await currentUserResolver.ResolveAsync();
        if (string.IsNullOrWhiteSpace(user.StripeCustomerId))
            return Ok(null);
        return Ok(await stripeAccountService.GetPaymentMethodDetailsAsync(user.StripeCustomerId));
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult<string>> CreateSetupIntent()
    {
        var user = await currentUserResolver.ResolveAsync();

        if (string.IsNullOrWhiteSpace(user.StripeCustomerId))
            await stripeAccountService.CreateCustomerAsync(user);

        return Ok(await stripeAccountService.CreateSetupIntentAsync(user.StripeCustomerId));
    }
}
