using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Concertable.Tests.Common;

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

    public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string url)
    {
        return await client.PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));
    }

    public static async Task<HttpResponseMessage> PostAsJsonEnsureSuccessAsync<T>(this HttpClient client, string url, T body)
    {
        var response = await client.PostAsync(url, body);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public static async Task<HttpResponseMessage> PostAsSuccessAsync(this HttpClient client, string url)
    {
        var response = await client.PostAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode}: {body}");
        }
        return response;
    }

    public static async Task<HttpResponseMessage> PostAsSuccessAsync<T>(this HttpClient client, string url, T body)
    {
        var response = await client.PostAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode}: {responseBody}");
        }
        return response;
    }

    public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string url, T body)
    {
        return await client.PutAsJsonAsync(url, body, JsonOptions);
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
