using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Concertable.Kernel.Auth;

internal sealed class ClientCredentialsTokenService : ITokenService
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<TokenServiceOptions> _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public ClientCredentialsTokenService(IHttpClientFactory factory, IOptions<TokenServiceOptions> options)
    {
        _factory = factory;
        _options = options;
    }

    public async Task<string> GetTokenAsync(string scope, CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _cachedToken;

            var opts = _options.Value;
            using var client = _factory.CreateClient();
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
            _cachedToken = doc.RootElement.GetProperty("access_token").GetString()!;
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
