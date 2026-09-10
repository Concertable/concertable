using System.Collections.Frozen;

namespace Concertable.Auth.Contracts;

/// <summary>
/// The interactive OAuth client roster — the single source for wire client ids and how a client id maps to
/// a <see cref="AuthParty"/>. Auth registers these with the identity server; downstream services classify
/// <see cref="Events.CredentialRegisteredEvent.ClientId"/> through <see cref="Find"/>.
/// </summary>
public static class InteractiveClients
{
    private static readonly FrozenDictionary<InteractiveClient, InteractiveClientInfo> ByClient = new[]
    {
        new InteractiveClientInfo(InteractiveClient.CustomerBrowser, "customer-web",     AuthParty.Customer, null),
        new InteractiveClientInfo(InteractiveClient.CustomerMobile,  "customer-mobile",  AuthParty.Customer, "concertable-customer://"),
        new InteractiveClientInfo(InteractiveClient.VenueBrowser,    "venue-web",        AuthParty.Venue,    null),
        new InteractiveClientInfo(InteractiveClient.VenueMobile,     "venue-mobile",     AuthParty.Venue,    "concertable-business://"),
        new InteractiveClientInfo(InteractiveClient.ArtistBrowser,   "artist-web",       AuthParty.Artist,   null),
        new InteractiveClientInfo(InteractiveClient.ArtistMobile,    "artist-mobile",    AuthParty.Artist,   "concertable-business://"),
        new InteractiveClientInfo(InteractiveClient.Admin,           "admin",            AuthParty.Admin,    null),
        new InteractiveClientInfo(InteractiveClient.E2ETest,         "concertable-test", null,               null),
    }.ToFrozenDictionary(info => info.Client);

    private static readonly FrozenDictionary<string, InteractiveClientInfo> ById =
        ByClient.Values.ToFrozenDictionary(info => info.Id, StringComparer.Ordinal);

    /// <summary>Every registered interactive client.</summary>
    public static IReadOnlyCollection<InteractiveClientInfo> All => ByClient.Values;

    /// <summary>The catalog row for a known client.</summary>
    public static InteractiveClientInfo Info(this InteractiveClient client) => ByClient[client];

    /// <summary>
    /// Resolves a wire client id (e.g. <see cref="Events.CredentialRegisteredEvent.ClientId"/>), or
    /// <see langword="null"/> for an id this build does not know — so a consumer skips an event from a
    /// client it predates rather than throwing.
    /// </summary>
    public static InteractiveClientInfo? Find(string clientId) =>
        ById.TryGetValue(clientId, out var info) ? info : null;
}
