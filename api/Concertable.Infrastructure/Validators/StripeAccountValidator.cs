using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Concertable.Identity.Contracts;

namespace Concertable.Infrastructure.Validators;

public class StripeAccountValidator : IStripeValidationStrategy
{
    private readonly ICurrentUser currentUser;
    private readonly IManagerModule managerModule;
    private readonly IStripeAccountService stripeAccountService;

    public StripeAccountValidator(ICurrentUser currentUser, IManagerModule managerModule, IStripeAccountService stripeAccountService)
    {
        this.currentUser = currentUser;
        this.managerModule = managerModule;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<bool> ValidateAsync()
    {
        var manager = await managerModule.GetByIdAsync(currentUser.GetId());
        if (manager is null) return false;

        return await stripeAccountService.GetAccountStatusAsync(manager.StripeAccountId) == PayoutAccountStatus.Verified;
    }
}
