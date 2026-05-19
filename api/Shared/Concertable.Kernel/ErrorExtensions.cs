using FluentResults;

namespace Concertable.Shared;

public static class ErrorExtensions
{
    public static IEnumerable<string> SelectMessages(this IEnumerable<IError> errors)
        => errors.Select(e => e.Message);
}
