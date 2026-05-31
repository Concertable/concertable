using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace Concertable.E2ETests;

public sealed class AspireResourceLogger : IAsyncDisposable
{
    private readonly CancellationTokenSource cts = new();
    private readonly Task task;

    public AspireResourceLogger(ResourceNotificationService notifications, ILogger logger)
    {
        task = Task.Run(async () =>
        {
            try
            {
                await foreach (var e in notifications.WatchAsync(cts.Token))
                    logger.AspireResourceStateChanged(e.Resource.Name, e.Snapshot.State?.Text ?? "unknown");
            }
            catch (OperationCanceledException) { }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await cts.CancelAsync();
        await task;
        cts.Dispose();
    }
}
