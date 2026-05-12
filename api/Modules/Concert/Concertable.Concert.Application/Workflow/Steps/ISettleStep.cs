namespace Concertable.Concert.Application.Workflow.Steps;

internal interface ISettleStep : IConcertStep
{
    Task ExecuteAsync(int bookingId);
}
