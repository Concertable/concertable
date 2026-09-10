using System.Collections.Immutable;

namespace Concertable.Auth.Contracts;

/// <summary>
/// Catalog row for an <see cref="AuthResource"/>: the JWT audience a resource server validates, the scopes it
/// accepts and the user claims Auth includes in its tokens. Obtain from <see cref="AuthResources"/>.
/// </summary>
public readonly record struct AuthResourceInfo(
    AuthResource Resource,
    string Audience,
    ImmutableArray<AuthScope> Scopes,
    ImmutableArray<string> UserClaims);
