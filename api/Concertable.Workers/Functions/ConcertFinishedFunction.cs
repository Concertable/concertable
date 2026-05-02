using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Workers.Functions;

internal class ConcertFinishedFunction
{
    private readonly IConcertRepository concertRepository;
    private readonly ICompletionDispatcher CompletionDispatcher;
    private readonly ILogger<ConcertFinishedFunction> logger;

    public ConcertFinishedFunction(
        IConcertRepository concertRepository,
        ICompletionDispatcher CompletionDispatcher,
        ILogger<ConcertFinishedFunction> logger)
    {
        this.concertRepository = concertRepository;
        this.CompletionDispatcher = CompletionDispatcher;
        this.logger = logger;
    }

    [Function(nameof(ConcertFinishedFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        var concertIds = await concertRepository.GetEndedConfirmedIdsAsync();

        foreach (var concertId in concertIds)
        {
            var result = await CompletionDispatcher.FinishAsync(concertId);

            if (result.IsFailed)
                logger.LogError("Failed to finish concert {ConcertId}: {Errors}", concertId, result.Errors);
            else
                logger.LogInformation("Finished concert {ConcertId}", concertId);
        }
    }
}
