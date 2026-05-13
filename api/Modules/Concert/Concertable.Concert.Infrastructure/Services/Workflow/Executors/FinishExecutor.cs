using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Executors;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class FinishExecutor : IFinishExecutor
{
    private readonly IStepExecutor<ConcertEntity> stepExecutor;
    private readonly IConcertWorkflowFactory workflows;
    private readonly ILogger<FinishExecutor> logger;

    public FinishExecutor(
        IStepExecutor<ConcertEntity> stepExecutor,
        IConcertWorkflowFactory workflows,
        ILogger<FinishExecutor> logger)
    {
        this.stepExecutor = stepExecutor;
        this.workflows = workflows;
        this.logger = logger;
    }

    public async Task<Result> ExecuteAsync(int concertId)
    {
        try
        {
            await stepExecutor.ExecuteAsync(concertId, ConcertStage.Finished, Dispatch);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to finish concert {ConcertId}", concertId);
            return Result.Fail(ex.Message);
        }
    }

    private Task Dispatch(ConcertEntity concert)
    {
        var workflow = workflows.Create(concert.ContractType);
        return workflow.Finish.ExecuteAsync(concert.Id);
    }
}
