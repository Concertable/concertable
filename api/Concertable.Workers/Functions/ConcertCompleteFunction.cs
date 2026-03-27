using Application.Interfaces.Concert;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Workers.Functions;

public class ConcertCompleteFunction
{
    private readonly IConcertRepository concertRepository;
    private readonly ICompleteProcessor completeProcessor;
    private readonly ILogger<ConcertCompleteFunction> logger;

    public ConcertCompleteFunction(
        IConcertRepository concertRepository,
        ICompleteProcessor completeProcessor,
        ILogger<ConcertCompleteFunction> logger)
    {
        this.concertRepository = concertRepository;
        this.completeProcessor = completeProcessor;
        this.logger = logger;
    }

    [Function(nameof(ConcertCompleteFunction))]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer)
    {
        var concertIds = await concertRepository.GetEndedConfirmedIdsAsync();

        foreach (var concertId in concertIds)
        {
            try
            {
                await completeProcessor.CompleteAsync(concertId);
                logger.LogInformation("Completed concert {ConcertId}", concertId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to complete concert {ConcertId}", concertId);
            }
        }
    }
}
