using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Concertable.Identity.Contracts;

namespace Concertable.Infrastructure.Validators;

public class StripeAccountValidator : IStripeValidationStrategy
{
    private readonly ICurrentUser currentUser;
    private readonly IIdentityModule identityModule;
    private readonly IStripeAccountService stripeAccountService;

    public StripeAccountValidator(ICurrentUser currentUser, IIdentityModule identityModule, IStripeAccountService stripeAccountService)
    {
        this.currentUser = currentUser;
        this.identityModule = identityModule;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<bool> ValidateAsync()
    {
        var manager = await identityModule.GetManagerAsync(currentUser.GetId());
        if (manager is null) return false;

        return await stripeAccountService.GetAccountStatusAsync(manager.StripeAccountId) == PayoutAccountStatus.Verified;
    }
}
