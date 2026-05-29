namespace Concertable.E2ETests;

public interface IHealthWaiter
{
    Task WaitForReadyAsync(TimeSpan timeout);
}
