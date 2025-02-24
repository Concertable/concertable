using Core.Entities.Identity;
using Core.Responses;
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
        Task<string> CreateBankAccountTokenAsync(string stripeId, int accountNo, int sortCode);
        Task<AddedBankAccountResponse> AddBankAccountAsync(string stripeId, string accountToken);
        Task SetupBankAccountAsync(string stripeId, int accountNo, int sortCode);
        Task<bool> IsUserVerifiedAsync(string stripeId);
    }
}
