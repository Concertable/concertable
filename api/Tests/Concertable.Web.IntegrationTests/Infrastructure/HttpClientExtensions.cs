using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string url, T body)
    {
        return await client.PostAsJsonAsync(url, body, JsonOptions);
    }

    public static async Task<HttpResponseMessage> PostAsJsonEnsureSuccessAsync(this HttpClient client, string url)
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode} from {url}: {body}");
        }
        return response;
    }

    public static async Task<HttpResponseMessage> PostAsJsonEnsureSuccessAsync<T>(this HttpClient client, string url, T body)
    {
        var response = await client.PostAsync(url, body);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string url, T body)
    {
        return await client.PutAsJsonAsync(url, body, JsonOptions);
    }

    public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, string url)
    {
        return await client.GetAsync(url);
    }

    public static async Task<T?> GetAsync<T>(this HttpClient client, string url)
    {
        return await client.GetFromJsonAsync<T>(url, JsonOptions);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string url)
    {
        return await client.DeleteAsync(url);
    }

    public static async Task<T?> ReadAsync<T>(this HttpContent content)
    {
        return await content.ReadFromJsonAsync<T>(JsonOptions);
    }
}
