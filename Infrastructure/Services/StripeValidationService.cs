using Application.Interfaces;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class StripeValidationService : IStripeValidationService
{
    private readonly ICurrentUser currentUser;
    private readonly IStripeAccountService stripeAccountService;

    public StripeValidationService(
        ICurrentUser currentUser,
        IStripeAccountService stripeAccountService
        )
    {
        this.stripeAccountService = stripeAccountService;
        this.currentUser = currentUser;
    }

    public async Task ValidateUserAsync()
    {
        var user = currentUser.GetEntity();

        if (user.StripeId is null)
            throw new UnauthorizedAccessException("You do not have a Stripe Id. Contact Support to get one");

        if (!await stripeAccountService.IsUserVerifiedAsync(user.StripeId))
            throw new UnauthorizedAccessException("You must create a Stripe Account first");
    }
}
