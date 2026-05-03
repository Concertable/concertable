namespace Concertable.Concert.Application.Interfaces;

internal interface ISettleable : IConcertWorkflowStep
{
    Task SettleAsync(int bookingId);
}
