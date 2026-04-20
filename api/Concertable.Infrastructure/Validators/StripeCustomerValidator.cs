using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;

namespace Concertable.Infrastructure.Validators;

public class StripeCustomerValidator : IStripeValidationStrategy
{
    private readonly ICurrentUserResolver currentUserResolver;

    public StripeCustomerValidator(ICurrentUserResolver currentUserResolver)
    {
        this.currentUserResolver = currentUserResolver;
    }

    public async Task<bool> ValidateAsync()
    {
        var user = await currentUserResolver.ResolveAsync();
        return !string.IsNullOrEmpty(user.StripeCustomerId);
    }
}
