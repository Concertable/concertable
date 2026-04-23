using Concertable.Concert.Application.Responses;
using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface IFinishedDispatcher
{
    Task<Result<IFinishOutcome>> FinishedAsync(int concertId);
}
