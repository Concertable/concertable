using Concertable.Application.Interfaces.Payment;
using Concertable.Identity.Contracts;

namespace Concertable.Infrastructure.Validators;

public class StripeCustomerValidator : IStripeValidationStrategy
{
    private readonly IAuthModule authModule;
    private readonly IManagerModule managerModule;
    private readonly ICurrentUser currentUser;

    public StripeCustomerValidator(IAuthModule authModule, IManagerModule managerModule, ICurrentUser currentUser)
    {
        this.authModule = authModule;
        this.managerModule = managerModule;
        this.currentUser = currentUser;
    }

    public async Task<bool> ValidateAsync()
    {
        var userId = currentUser.GetId();

        var manager = await managerModule.GetByIdAsync(userId);
        if (manager is not null) return !string.IsNullOrEmpty(manager.StripeCustomerId);

        var customer = await authModule.GetCustomerAsync(userId);
        return !string.IsNullOrEmpty(customer?.StripeCustomerId);
    }
}
