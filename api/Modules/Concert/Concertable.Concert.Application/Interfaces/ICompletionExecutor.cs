using Concertable.Concert.Application.Responses;
using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface ICompletionExecutor
{
    Task<Result<IFinishOutcome>> FinishAsync(int concertId);
}
