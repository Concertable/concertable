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
                        _ = StreamResourceLogsAsync(loggers, e.Resource, logger);
                }
            }
            catch (OperationCanceledException) { }
        });
    }

    private async Task StreamResourceLogsAsync(ResourceLoggerService loggers, IResource resource, ILogger logger)
    {
        try
        {
            await foreach (var batch in loggers.WatchAsync(resource).WithCancellation(cts.Token))
                foreach (var line in batch)
                    logger.AspireResourceLog(resource.Name, line.Content);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.AspireResourceLog(resource.Name, $"[log-stream error] {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await cts.CancelAsync();
        await task;
        cts.Dispose();
    }
}
