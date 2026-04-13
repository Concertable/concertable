namespace Concertable.Web.E2ETests.Infrastructure;

public static class HttpClientExtensions
{
    public static async Task<T> PollUntilAsync<T>(
        this HttpClient client,
        string url,
        Func<T, bool> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        T? result = default;

        await WaitHelper.UntilAsync(async () =>
        {
            result = await client.GetAsync<T>(url);
            return result is not null && condition(result);
        }, timeout, interval);

        return result!;
    }
}
