using System.Collections.Frozen;

namespace Concertable.Auth.Contracts;

/// <summary>Maps <see cref="AuthScope"/> to and from its wire string — the one place the scope literals live.</summary>
public static class AuthScopes
{
    private static readonly FrozenDictionary<AuthScope, string> ByScope = new Dictionary<AuthScope, string>
    {
        [AuthScope.B2BApi] = "concertable.b2b.api",
        [AuthScope.CustomerApi] = "concertable.customer.api",
        [AuthScope.SearchApi] = "concertable.search.api",
        [AuthScope.PaymentWrite] = "payment:write",
        [AuthScope.UserClaims] = "user:claims",
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, AuthScope> ById =
        ByScope.ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    /// <summary>The wire scope string, e.g. <c>concertable.b2b.api</c>.</summary>
    public static string Id(this AuthScope scope) => ByScope[scope];

    /// <summary>Resolves a wire scope string; an unknown value returns <see langword="false"/>.</summary>
    public static bool TryGet(string scope, out AuthScope value) => ById.TryGetValue(scope, out value);

    /// <summary>Every issued scope.</summary>
    public static IReadOnlyCollection<AuthScope> All => ByScope.Keys;
}
