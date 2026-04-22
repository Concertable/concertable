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
    private readonly IManagerModule managerModule;
    private readonly IAuthModule authModule;

    public StripeAccountController(
        IStripeAccountService stripeAccountService,
        ICurrentUser currentUser,
        IManagerModule managerModule,
        IAuthModule authModule)
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
        this.managerModule = managerModule;
        this.authModule = authModule;
    }

    [HttpGet("onboarding-link")]
    public async Task<ActionResult<string>> GetOnboardingLink()
    {
        var manager = await managerModule.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        return Ok(await stripeAccountService.GetOnboardingLinkAsync(manager.StripeAccountId));
    }

    [HttpGet("account-status")]
    public async Task<ActionResult<PayoutAccountStatus>> GetAccountStatus()
    {
        var manager = await managerModule.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        return Ok(await stripeAccountService.GetAccountStatusAsync(manager.StripeAccountId));
    }

    [HttpGet("payment-method")]
    public async Task<ActionResult<PaymentMethodDto?>> GetPaymentMethod()
    {
        var customer = await authModule.GetCustomerAsync(currentUser.GetId());
        if (customer is null || string.IsNullOrWhiteSpace(customer.StripeCustomerId))
            return Ok(null);
        return Ok(await stripeAccountService.GetPaymentMethodDetailsAsync(customer.StripeCustomerId));
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult<string>> CreateSetupIntent()
    {
        var customer = await authModule.GetCustomerAsync(currentUser.GetId());
        if (customer is null) return Unauthorized();

        var stripeCustomerId = customer.StripeCustomerId;
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
        {
            stripeCustomerId = await stripeAccountService.CreateCustomerAsync(customer.Email!);
            await authModule.SetStripeCustomerIdAsync(currentUser.GetId(), stripeCustomerId);
        }

        return Ok(await stripeAccountService.CreateSetupIntentAsync(stripeCustomerId));
    }
}
