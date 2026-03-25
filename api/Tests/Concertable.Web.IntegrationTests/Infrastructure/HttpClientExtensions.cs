using System.Net.Http.Json;

namespace Concertable.Web.IntegrationTests.Infrastructure;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string url, T body)
    {
        return await client.PostAsJsonAsync(url, body);
    }

    public static async Task<TResponse?> PostAsync<TBody, TResponse>(this HttpClient client, string url, TBody body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string url, T body)
    {
        return await client.PutAsJsonAsync(url, body);
    }

    public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, string url)
    {
        return await client.GetAsync(url);
    }

    public static async Task<T?> GetAsync<T>(this HttpClient client, string url)
    {
        return await client.GetFromJsonAsync<T>(url);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string url)
    {
        return await client.DeleteAsync(url);
    }
}
