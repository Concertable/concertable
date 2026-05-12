namespace Concertable.Concert.Application.Workflow.Steps;

internal interface ISimpleApplyStep : IConcertStep
{
    static ConcertStage IConcertStep.Stage => ConcertStage.Applied;
    Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId);
}
