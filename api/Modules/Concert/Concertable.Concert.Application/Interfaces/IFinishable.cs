using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IFinishable
{
    Task<IFinishOutcome> FinishAsync(int concertId);
}
