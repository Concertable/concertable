using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Validators;

public class StripeValidator : IStripeValidator
{
    private readonly ICurrentUser currentUser;
    private readonly IStripeAccountService stripeAccountService;

    public StripeValidator(
        ICurrentUser currentUser,
        IStripeAccountService stripeAccountService)
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
    }

    public async Task<ValidationResult> ValidateUserAsync()
    {
        var result = new ValidationResult();
        var user = currentUser.GetEntity();

        if (user.StripeId is null)
        {
            result.AddError("You do not have a Stripe Id. Contact Support to get one");
            return result;
        }

        if (!await stripeAccountService.IsUserVerifiedAsync(user.StripeId))
            result.AddError("You must create a Stripe Account first");

        return result;
    }
}
