namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IFinishStep : IConcertStep
{
    Task ExecuteAsync(int concertId);
}
