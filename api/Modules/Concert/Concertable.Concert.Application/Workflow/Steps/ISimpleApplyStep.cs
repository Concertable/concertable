namespace Concertable.Concert.Application.Workflow.Steps;

internal interface ISimpleApplyStep : IConcertStep
{
    Task<ApplicationEntity> ExecuteAsync(int artistId, int opportunityId);
}
