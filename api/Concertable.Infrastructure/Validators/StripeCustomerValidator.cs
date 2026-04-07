using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Validators;

public class StripeCustomerValidator : IStripeValidationStrategy
{
    private readonly ICurrentUser currentUser;

    public StripeCustomerValidator(ICurrentUser currentUser)
    {
        this.currentUser = currentUser;
    }

    public Task<bool> ValidateAsync()
    {
        var manager = currentUser.GetEntity<ManagerEntity>();
        return Task.FromResult(manager.StripeCustomerId is not null);
    }
}
