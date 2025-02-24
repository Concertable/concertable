using Application.Interfaces;
using Core.Entities.Identity;
using Core.Responses;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.IsisMtt.Ocsp;
using Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class StripeAccountService : IStripeAccountService
    {
        private readonly StripeSettings stripeSettings;
        private readonly IUserRepository userRepository;
        private readonly string baseUri;

        public StripeAccountService(
            IUserRepository userRepository, 
            IOptions<StripeSettings> stripeSettings,
            IConfiguration configuration) 
        {
            this.stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
            this.userRepository = userRepository;
            this.baseUri = configuration["BaseUri:http"];
        }

        public async Task<string> CreateStripeAccountAsync(ApplicationUser user)
        {
            var service = new AccountService();
            var options = new AccountCreateOptions
            {
                Type = "express",
                Email = user.Email,
                Country = "GB",
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                }
            };

            var account = await service.CreateAsync(options);

            user.StripeId = account.Id;
            userRepository.Update(user);
            await userRepository.SaveChangesAsync();

            return account.Id;
        }

        public async Task<string> CreateBankAccountTokenAsync(string stripeId, int accountNo, int sortCode)
        {
            var options = new TokenCreateOptions
            {
                BankAccount = new TokenBankAccountOptions {
                    Country = "GB",
                    Currency = "GBP",
                    AccountHolderType = "individual",
                    RoutingNumber = sortCode.ToString(),
                    AccountNumber = accountNo.ToString()
                }
            };
            var service = new TokenService();
            var token = await service.CreateAsync(options);

            return token.Id;
        }

        public async Task<AddedBankAccountResponse> AddBankAccountAsync(string stripeId, string accountToken)
        {
            var options = new AccountExternalAccountCreateOptions
            {
                ExternalAccount = accountToken
            };

            var service = new AccountExternalAccountService();
            var account = await service.CreateAsync(stripeId, options);

            var redirectUri = await CreateOnboardingLinkAsync(stripeId);

            return new AddedBankAccountResponse
            {
                AccountId = account.Id,
                RedirectUri = redirectUri
            };
        }

        private async Task<string> CreateOnboardingLinkAsync(string stripeId)
        {
            var isVerified = await IsUserVerifiedAsync(stripeId);
            var service = new AccountLinkService();
            var options = new AccountLinkCreateOptions
            {
                Account = stripeId,
                RefreshUrl = $"{baseUri}/reauth",
                ReturnUrl = $"{baseUri}/dashboard",
                Type = isVerified ? "account_update" : "account_onboarding"
            };

            var link = await service.CreateAsync(options);
            return link.Url; 
        }

        public async Task SetupBankAccountAsync(string stripeId, int accountNo, int sortCode)
        {
            var tokenId = await CreateBankAccountTokenAsync(stripeId, accountNo, sortCode);

            await AddBankAccountAsync(stripeId, tokenId);
        }

        public async Task<bool> IsUserVerifiedAsync(string stripeId)
        {
            var service = new AccountService();
            var account = await service.GetAsync(stripeId);

            return account.PayoutsEnabled && account.ChargesEnabled;
        }
    }
}
