using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace Concertable.E2ETests;

public sealed class AspireResourceLogger : IAsyncDisposable
{
    private readonly CancellationTokenSource cts = new();
    private readonly Task task;

    public AspireResourceLogger(ResourceNotificationService notifications, ResourceLoggerService loggers, ILogger logger)
    {
        task = Task.Run(async () =>
        {
            var streamed = new HashSet<string>();
            try
            {
                await foreach (var e in notifications.WatchAsync(cts.Token))
                {
                    logger.AspireResourceStateChanged(e.Resource.Name, e.Snapshot.State?.Text ?? "unknown");
                    if (streamed.Add(e.Resource.Name))
                        _ = StreamResourceLogsAsync(loggers, e.Resource.Name, logger);
                }
            }
            catch (OperationCanceledException) { }
        });
    }

    private async Task StreamResourceLogsAsync(ResourceLoggerService loggers, string name, ILogger logger)
    {
        try
        {
            await foreach (var batch in loggers.WatchAsync(name).WithCancellation(cts.Token))
                foreach (var line in batch)
                    logger.AspireResourceLog(name, line.Content);
        }
        catch (OperationCanceledException) { }
    }

    public async ValueTask DisposeAsync()
    {
        await cts.CancelAsync();
        await task;
        cts.Dispose();
    }
}
