namespace Concertable.Concert.Application.Interfaces;

internal interface IVerifiable : IConcertWorkflowStep
{
    Task VerifyAsync(int applicationId);
}
