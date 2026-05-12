using Microsoft.Azure.Functions.Worker;

namespace Workers.Functions;

internal class ConcertFinishedFunction(IConcertCompletionRunner runner)
{
    [Function(nameof(ConcertFinishedFunction))]
    public Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer) => runner.RunAsync();
}
