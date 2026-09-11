namespace Concertable.Auth.Contracts;

/// <summary>Superseded by <see cref="AuthScope"/> / <see cref="AuthScopes"/> and <see cref="AuthResource"/>. Removed once every consumer has migrated.</summary>
[Obsolete("Use AuthScope / AuthScopes (scopes) or AuthResource / AuthResources (audiences).")]
public static class ApiScopeIds
{
    public const string B2BApi = "concertable.b2b.api";
    public const string CustomerApi = "concertable.customer.api";
    public const string SearchApi = "concertable.search.api";
    public const string PaymentWrite = "payment:write";
    public const string UserClaims = "user:claims";
}
