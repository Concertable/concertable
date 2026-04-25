namespace Concertable.Shared;

public interface IBackgroundTaskRunner
{
    Task RunAsync(Func<CancellationToken, Task> work);

    Task RunAsync<TService>(
        Func<TService, CancellationToken, Task> work)
        where TService : notnull;
}
