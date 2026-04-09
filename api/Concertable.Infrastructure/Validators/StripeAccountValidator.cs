using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Validators;

public class StripeAccountValidator : IStripeValidationStrategy
{
    private readonly ICurrentUser currentUser;
    private readonly IStripeAccountService stripeAccountService;

    public StripeAccountValidator(ICurrentUser currentUser, IStripeAccountService stripeAccountService)
    {
        this.currentUser = currentUser;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<bool> ValidateAsync()
    {
        var manager = currentUser.GetEntity<ManagerEntity>();

        return await stripeAccountService.IsUserVerifiedAsync(manager.StripeAccountId);
    }
}
