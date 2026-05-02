namespace Concertable.Concert.Application.Interfaces;

internal interface IFinishable : IConcertWorkflowStep
{
    Task FinishAsync(int concertId);
}
