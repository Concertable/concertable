using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IFinishable : IConcertWorkflowStep
{
    Task<IFinishOutcome> FinishAsync(int concertId);
}
