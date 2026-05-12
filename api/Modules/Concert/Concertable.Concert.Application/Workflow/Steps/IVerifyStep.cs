namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IVerifyStep : IConcertStep
{
    Task ExecuteAsync(int applicationId);
}
