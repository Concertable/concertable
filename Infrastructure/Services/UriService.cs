using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UriService : IUriService
    {
        private readonly string baseUri;
        private IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly LinkGenerator linkGenerator;

        public UriService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
        {
            this.baseUri = configuration["BaseUri:http"];
            this.httpContextAccessor = httpContextAccessor;
            this.linkGenerator = linkGenerator;
        }

        public Uri GetEmailConfirmationUri(int userId, string token)
        {
            var uri = linkGenerator.GetUriByAction(
                httpContextAccessor.HttpContext!,
                action: "ConfirmEmail",
                controller: "Auth",
                values: new { userId, token }
            );

            if (uri is null)
                throw new ArgumentException("Failed to generate email confirmation link");

            return new Uri(uri);
        }

        public Uri GetEmailChangeConfirmationUri(int userId, string token, string newEmail)
        {
            var uri = linkGenerator.GetUriByAction(
                httpContextAccessor.HttpContext!,
                action: "ConfirmEmailChange",
                controller: "Auth",
                values: new { userId, token, newEmail }
            );

            if (uri is null)
                throw new ArgumentException("Failed to generate email change confirmation link");

            return new Uri(uri);
        }

        public Uri GetPasswordResetUri(int userId, string token)
        {
            var uri = linkGenerator.GetUriByAction(
                httpContextAccessor.HttpContext!,
                action: "ForgotPassword",
                controller: "Auth",
                values: new { userId, token }
            );

            if (uri is null)
                throw new ArgumentException("Failed to generate email confirmation link");

            return new Uri(uri);
        }
    }
}
