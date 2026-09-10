using System.Collections.Frozen;

namespace Concertable.Auth.Contracts;

/// <summary>
/// The resource-server roster — audience, accepted scopes and user claims per API. Auth registers these as
/// identity-server API resources; each resource server validates its own <see cref="AuthResourceInfo.Audience"/>.
/// </summary>
public static class AuthResources
{
    private static readonly FrozenDictionary<AuthResource, AuthResourceInfo> ByResource = new[]
    {
        new AuthResourceInfo(AuthResource.B2B, "concertable.b2b.api",
            [AuthScope.B2BApi], ["email"]),
        new AuthResourceInfo(AuthResource.Customer, "concertable.customer.api",
            [AuthScope.CustomerApi, AuthScope.UserClaims], ["role", "owner"]),
        new AuthResourceInfo(AuthResource.Search, "concertable.search.api",
            [AuthScope.SearchApi], []),
        new AuthResourceInfo(AuthResource.Payment, "concertable.payment.api",
            [AuthScope.PaymentWrite], []),
    }.ToFrozenDictionary(info => info.Resource);

    private static readonly FrozenDictionary<string, AuthResourceInfo> ByAudience =
        ByResource.Values.ToFrozenDictionary(info => info.Audience, StringComparer.Ordinal);

    /// <summary>The catalog row for a resource.</summary>
    public static AuthResourceInfo Info(this AuthResource resource) => ByResource[resource];

    /// <summary>Resolves a JWT audience string; an unknown value returns <see langword="false"/>.</summary>
    public static bool TryGet(string audience, out AuthResourceInfo info) => ByAudience.TryGetValue(audience, out info);

    /// <summary>Every resource server.</summary>
    public static IReadOnlyCollection<AuthResourceInfo> All => ByResource.Values;
}
