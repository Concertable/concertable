using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Concertable.Kernel.Auth;

internal sealed class ClientCredentialsTokenService : ITokenService
{
    private readonly IHttpClientFactory factory;
    private readonly IOptions<TokenServiceOptions> options;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly ConcurrentDictionary<string, (string Token, DateTimeOffset Expiry)> cache = new();

    public ClientCredentialsTokenService(IHttpClientFactory factory, IOptions<TokenServiceOptions> options)
    {
        this.factory = factory;
        this.options = options;
    }

    public async Task<string> GetTokenAsync(string scope, CancellationToken ct = default)
    {
        if (TryGetCached(scope, out var token))
            return token;

        await gate.WaitAsync(ct);
        try
        {
            if (TryGetCached(scope, out token))
                return token;

            var opts = options.Value;
            using var client = factory.CreateClient();
            using var response = await client.PostAsync(
                $"{opts.Authority.TrimEnd('/')}/connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = opts.ClientId,
                    ["client_secret"] = opts.ClientSecret,
                    ["scope"] = scope
                }), ct);

            response.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            token = doc.RootElement.GetProperty("access_token").GetString()!;
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            cache[scope] = (token, DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30));
            return token;
        }
        finally
        {
            gate.Release();
        }
    }

    private bool TryGetCached(string scope, out string token)
    {
        if (cache.TryGetValue(scope, out var entry) && DateTimeOffset.UtcNow < entry.Expiry)
        {
            token = entry.Token;
            return true;
        }
        token = null!;
        return false;
    }
}
