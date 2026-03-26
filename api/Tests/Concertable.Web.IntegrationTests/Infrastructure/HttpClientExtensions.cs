using System.Net.Http.Json;
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

    public static async Task<TResponse?> PostAsync<TBody, TResponse>(this HttpClient client, string url, TBody body)
    {
        var response = await client.PostAsJsonAsync(url, body, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
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

    public static Task<HttpResponseMessage> PostWebhookAsync(this HttpClient client, string json)
    {
        return client.PostAsync("/api/Webhook", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
    }
}
