using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Concertable.Infrastructure.Services;

public class UriService : IUriService
{
    private readonly UrlSettings _urlSettings;

    public UriService(IOptions<UrlSettings> urlSettings)
    {
        _urlSettings = urlSettings.Value;
    }

    public Uri GetUri(string path, IDictionary<string, string>? query = null)
    {
        var builder = new UriBuilder(_urlSettings.Frontend) { Path = path };

        if (query?.Count > 0)
            builder.Query = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

        return builder.Uri;
    }
}
