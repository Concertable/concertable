using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface IFinishedProcessor
{
    Task<Result> FinishedAsync(int concertId);
}
