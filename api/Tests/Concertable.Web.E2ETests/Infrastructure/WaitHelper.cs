namespace Concertable.Web.E2ETests.Infrastructure;

public static class WaitHelper
{
    public static async Task UntilAsync(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        interval ??= TimeSpan.FromMilliseconds(250);

        using var cts = new CancellationTokenSource(timeout.Value);

        while (!cts.IsCancellationRequested)
        {
            try
            {
                if (await condition()) return;
            }
            catch { }

            try
            {
                await Task.Delay(interval.Value, cts.Token);
            }
            catch (OperationCanceledException) { break; }
        }

        throw new TimeoutException("Condition was not met within the timeout.");
    }
}
