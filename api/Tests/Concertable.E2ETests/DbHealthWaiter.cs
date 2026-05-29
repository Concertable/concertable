using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.E2ETests;

public sealed class DbHealthWaiter
{
    private readonly ILogger<DbHealthWaiter> logger;

    public DbHealthWaiter(ILogger<DbHealthWaiter> logger)
    {
        this.logger = logger;
    }

    public async Task WaitForCountAsync<T>(IQueryable<T> query, int expectedCount, TimeSpan timeout) where T : class
    {
        using var cts = new CancellationTokenSource(timeout);
        int? lastCount = null;
        try
        {
            while (true)
            {
                var current = await query.CountAsync(cts.Token);
                if (current != lastCount)
                {
                    logger.DbHealthWaiterProgress(typeof(T).Name, current, expectedCount);
                    lastCount = current;
                }
                if (current >= expectedCount) return;
                await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Timed out waiting for {expectedCount} {typeof(T).Name} rows; last observed count {lastCount}.");
        }
    }
}
