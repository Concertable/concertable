using System.Net;

namespace Concertable.Kernel.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string detail) : base(detail, HttpStatusCode.Forbidden)
    {
        Title = "Forbidden";
    }
}
