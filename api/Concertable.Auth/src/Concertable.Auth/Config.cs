using System.Security.Cryptography;
using System.Text;
using Concertable.Auth.Contracts;
using Concertable.Auth.Settings;
using Duende.IdentityServer.Models;

namespace Concertable.Auth;

public static class Config
{
    public static IReadOnlyList<ApiScope> ApiScopes =>
    [
        new ApiScope(ApiScopeIds.B2BApi,       "Concertable B2B API"),
        new ApiScope(ApiScopeIds.CustomerApi,  "Concertable Customer API"),
        new ApiScope(ApiScopeIds.SearchApi,    "Concertable Search API"),
        new ApiScope(ApiScopeIds.PaymentWrite, "Payment write access"),
        new ApiScope(ApiScopeIds.UserClaims,   "User claims access"),
    ];

    public static IReadOnlyList<ApiResource> ApiResources =>
    [
        /* B2B is identity-only: `email` comes from the local Auth credential, and authority is the
           request-scoped active tenant (X-Tenant-Id → membership), never a token claim. No `role`, no
           `owner` — one claim can't model a multi-tenant user. `owner` stays Customer-only. */
        new ApiResource(ApiScopeIds.B2BApi, "Concertable B2B API")
        {
            Scopes = { ApiScopeIds.B2BApi },
            UserClaims = { "email" }
        },
        new ApiResource(ApiScopeIds.CustomerApi, "Concertable Customer API")
        {
            Scopes = { ApiScopeIds.CustomerApi, ApiScopeIds.UserClaims },
            UserClaims = { "role", "owner" }
        },
        new ApiResource(ApiScopeIds.SearchApi, "Concertable Search API")
        {
            Scopes = { ApiScopeIds.SearchApi }
        },
        new ApiResource("concertable.payment.api", "Concertable Payment API")
        {
            Scopes = { ApiScopeIds.PaymentWrite }
        },
    ];

    public static IReadOnlyList<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource("roles", new[] { "role" }),
    ];

    public static Client CustomerMobileClient(string? expoGoRedirectUri = null) =>
        MobileClient(ClientIds.CustomerMobile, "concertable-customer://", expoGoRedirectUri);

    public static Client VenueMobileClient(string? expoGoRedirectUri = null) =>
        MobileClient(ClientIds.VenueMobile, "concertable-business://", expoGoRedirectUri);

    public static Client ArtistMobileClient(string? expoGoRedirectUri = null) =>
        MobileClient(ClientIds.ArtistMobile, "concertable-business://", expoGoRedirectUri);

    private static Client MobileClient(string clientId, string scheme, string? expoGoRedirectUri)
    {
        var redirectUris = new HashSet<string> { scheme };
        if (!string.IsNullOrEmpty(expoGoRedirectUri))
            redirectUris.Add(expoGoRedirectUri);

        return new Client
        {
            ClientId = clientId,

            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,

            RedirectUris = redirectUris,
            PostLogoutRedirectUris = { scheme },

            AllowedScopes = clientId == ClientIds.CustomerMobile
                ? new HashSet<string> { "openid", "profile", ApiScopeIds.CustomerApi }
                : new HashSet<string> { "openid", "profile", ApiScopeIds.B2BApi },

            AllowOfflineAccess = true,
            AccessTokenLifetime = 900,

            RefreshTokenUsage = TokenUsage.OneTimeOnly,
            RefreshTokenExpiration = TokenExpiration.Sliding,
            SlidingRefreshTokenLifetime = 60 * 60 * 24 * 30
        };
    }

    public static Client TestClient => new Client
    {
        ClientId = ClientIds.Test,
        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
        RequireClientSecret = false,
        AllowedScopes = { "openid", ApiScopeIds.B2BApi, ApiScopeIds.CustomerApi, ApiScopeIds.SearchApi },
    };

    public static IReadOnlyList<Client> WebClients(SpaClientSettings spa)
    {
        (string Name, string ClientId, WebClientSettings Settings)[] definitions =
        [
            (nameof(SpaClientSettings.Customer), ClientIds.CustomerWeb, spa.Customer),
            (nameof(SpaClientSettings.Venue), ClientIds.VenueWeb, spa.Venue),
            (nameof(SpaClientSettings.Artist), ClientIds.ArtistWeb, spa.Artist),
            (nameof(SpaClientSettings.Admin), ClientIds.Admin, spa.Admin),
        ];

        if (!spa.RestrictToEnabledClients)
            return definitions.Select(definition => WebClient(definition.ClientId, definition.Settings)).ToArray();

        var enabled = spa.EnabledClients?.ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];
        var unknown = enabled
            .Except(definitions.Select(definition => definition.Name), StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (unknown.Length > 0)
            throw new InvalidOperationException($"Unknown Auth SPA clients: {string.Join(", ", unknown)}.");

        return definitions
            .Where(definition => enabled.Contains(definition.Name))
            .Select(definition => WebClient(definition.ClientId, definition.Settings))
            .ToArray();
    }

    private static Client WebClient(string clientId, WebClientSettings settings) => new()
    {
        ClientId = clientId,

        AllowedGrantTypes = GrantTypes.Code,
        RequirePkce = true,
        RequireClientSecret = false,

        RedirectUris = [settings.RedirectUri],
        PostLogoutRedirectUris = [settings.PostLogoutRedirectUri],
        AllowedCorsOrigins = settings.AllowedCorsOrigins,

        AllowedScopes = clientId == ClientIds.CustomerWeb
            ? new HashSet<string> { "openid", "profile", "roles", ApiScopeIds.CustomerApi, ApiScopeIds.SearchApi }
            : new HashSet<string> { "openid", "profile", ApiScopeIds.B2BApi },

        AllowOfflineAccess = true,
        AccessTokenLifetime = 900,

        RefreshTokenUsage = TokenUsage.OneTimeOnly,
        RefreshTokenExpiration = TokenExpiration.Sliding,
        SlidingRefreshTokenLifetime = 60 * 60 * 24 * 30
    };

    public static Client ServiceClient(string clientId, string clientSecret, params string[] allowedScopes) => new()
    {
        ClientId = clientId,
        ClientSecrets = { new Secret(Sha256(clientSecret)) },
        AllowedGrantTypes = GrantTypes.ClientCredentials,
        AllowedScopes = allowedScopes.ToList()
    };

    private static string Sha256(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }
}
