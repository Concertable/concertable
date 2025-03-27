using Core.Entities.Identity;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStripeAccountService
    {
        Task<string> CreateStripeAccountAsync(ApplicationUser user);
        Task<string> GetOnboardingLinkAsync(string stripeId);
        Task<bool> IsUserVerifiedAsync(string stripeId);
    }
}
