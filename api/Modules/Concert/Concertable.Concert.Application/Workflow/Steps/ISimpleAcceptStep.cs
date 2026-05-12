namespace Concertable.Concert.Application.Workflow.Steps;

internal interface ISimpleAcceptStep : IConcertStep
{
    Task ExecuteAsync(int applicationId);
}
