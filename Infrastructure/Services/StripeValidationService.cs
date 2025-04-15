using Application.Interfaces;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class StripeValidationService : IStripeValidationService
    {
        private readonly ICurrentUserService currentUserService;
        private readonly IStripeAccountService stripeAccountService;

        public StripeValidationService(
            ICurrentUserService currentUserService,
            IStripeAccountService stripeAccountService
            )
        {
            this.stripeAccountService = stripeAccountService;
            this.currentUserService = currentUserService;
        }

        public async Task ValidateUserAsync()
        {
            var user = await currentUserService.GetEntityAsync();

            if (user.StripeId is null)
                throw new UnauthorizedAccessException("You must have a Stripe Account. Contact Support to get one");

            if (!await stripeAccountService.IsUserVerifiedAsync(user.StripeId))
                throw new UnauthorizedAccessException("You must create a Stripe Account first");
        }
    }
}
