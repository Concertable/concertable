using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Concertable.Kernel.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string detail) : base(detail, HttpStatusCode.Forbidden)
    {
        Title = "Forbidden";
    }

    public static void ThrowIfNull([NotNull] object? argument, string message)
    {
        if (argument is null)
            throw new ForbiddenException(message);
    }
}
