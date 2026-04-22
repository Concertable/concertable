using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Workers.Functions;

public class ConcertFinishedFunction
{
    private readonly IConcertRepository concertRepository;
    private readonly IFinishedDispatcher finishedDispatcher;
    private readonly ILogger<ConcertFinishedFunction> logger;

    public ConcertFinishedFunction(
        IConcertRepository concertRepository,
        IFinishedDispatcher finishedDispatcher,
        ILogger<ConcertFinishedFunction> logger)
    {
        this.concertRepository = concertRepository;
        this.finishedDispatcher = finishedDispatcher;
        this.logger = logger;
    }

    [Function(nameof(ConcertFinishedFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        var concertIds = await concertRepository.GetEndedConfirmedIdsAsync();

        foreach (var concertId in concertIds)
        {
            var result = await finishedDispatcher.FinishedAsync(concertId);

            if (result.IsFailed)
                logger.LogError("Failed to finish concert {ConcertId}: {Errors}", concertId, result.Errors);
            else
                logger.LogInformation("Finished concert {ConcertId}", concertId);
        }
    }
}
