namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IPaidAcceptStep : IConcertStep
{
    Task ExecuteAsync(int applicationId, string paymentMethodId);
}
