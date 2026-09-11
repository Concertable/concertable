namespace Concertable.Auth.Contracts;

/// <summary>
/// Catalog row for an <see cref="InteractiveClient"/>: its wire client id, the party it serves
/// (<see langword="null"/> only for the party-agnostic E2E client) and, for a native app, its redirect
/// scheme. Obtain from <see cref="InteractiveClients"/>; never construct one.
/// </summary>
public readonly record struct InteractiveClientInfo(
    InteractiveClient Client,
    string Id,
    AuthParty? Party,
    string? MobileScheme)
{
    /// <summary>A native mobile app rather than a browser SPA.</summary>
    public bool IsMobile => MobileScheme is not null;

    /// <summary>A B2B portal client — venue, artist or platform admin.</summary>
    public bool IsB2b => Party is AuthParty.Venue or AuthParty.Artist or AuthParty.Admin;
}
