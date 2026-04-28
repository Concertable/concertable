using Concertable.Authorization.Contracts;
using Concertable.Customer.Contracts;
using Concertable.Identity.Contracts;
using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Payment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class StripeAccountController : ControllerBase
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly ICurrentUser currentUser;
    private readonly IManagerModule managerModule;
    private readonly ICustomerModule customerModule;
    private readonly IPayoutAccountRepository payoutAccountRepository;

    public StripeAccountController(
        IStripeAccountService stripeAccountService,
        ICurrentUser currentUser,
        IManagerModule managerModule,
        ICustomerModule customerModule,
        IPayoutAccountRepository payoutAccountRepository)
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
        this.managerModule = managerModule;
        this.customerModule = customerModule;
        this.payoutAccountRepository = payoutAccountRepository;
    }

    [HttpGet("onboarding-link")]
    public async Task<ActionResult<string>> GetOnboardingLink()
    {
        _ = await managerModule.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        var account = await payoutAccountRepository.GetByUserIdAsync(currentUser.GetId());
        if (account?.StripeAccountId is null) return BadRequest("No Stripe connect account found.");

        return Ok(await stripeAccountService.GetOnboardingLinkAsync(account.StripeAccountId));
    }

    [HttpGet("account-status")]
    public async Task<ActionResult<PayoutAccountStatus>> GetAccountStatus()
    {
        _ = await managerModule.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("Manager not found.");

        var account = await payoutAccountRepository.GetByUserIdAsync(currentUser.GetId());
        if (account?.StripeAccountId is null) return Ok(PayoutAccountStatus.NotVerified);

        return Ok(await stripeAccountService.GetAccountStatusAsync(account.StripeAccountId));
    }

    [HttpGet("payment-method")]
    public async Task<ActionResult<PaymentMethodDto?>> GetPaymentMethod()
    {
        var account = await payoutAccountRepository.GetByUserIdAsync(currentUser.GetId());
        if (account?.StripeCustomerId is null) return Ok(null);

        return Ok(await stripeAccountService.GetPaymentMethodDetailsAsync(account.StripeCustomerId));
    }

    [HttpPost("setup-intent")]
    public async Task<ActionResult<string>> CreateSetupIntent()
    {
        if (currentUser.Id is null)
            return Ok(await stripeAccountService.CreateSetupIntentAsync(null));

        var userId = currentUser.GetId();
        var customer = await customerModule.GetCustomerAsync(userId);
        if (customer is null) return Unauthorized();

        var account = await payoutAccountRepository.GetByUserIdAsync(userId);
        var stripeCustomerId = account?.StripeCustomerId;

        if (string.IsNullOrWhiteSpace(stripeCustomerId))
        {
            await stripeAccountService.ProvisionCustomerAsync(userId, customer.Email!);
            account = await payoutAccountRepository.GetByUserIdAsync(userId);
            stripeCustomerId = account?.StripeCustomerId
                ?? throw new InvalidOperationException("Failed to provision Stripe customer.");
        }

        return Ok(await stripeAccountService.CreateSetupIntentAsync(stripeCustomerId));
    }
}
