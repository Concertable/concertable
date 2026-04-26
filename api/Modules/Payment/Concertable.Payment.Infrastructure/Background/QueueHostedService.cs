using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Background;

internal class QueueHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue taskQueue;
    private readonly ILogger<QueueHostedService> logger;

    public QueueHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueueHostedService> logger)
    {
        this.taskQueue = taskQueue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(cancellationToken);
            try
            {
                await workItem(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing background work item.");
            }
        }
    }
}
