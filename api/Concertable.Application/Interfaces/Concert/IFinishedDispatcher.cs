using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IFinishedDispatcher
{
    Task<Result<IFinishOutcome>> FinishedAsync(int concertId);
}
