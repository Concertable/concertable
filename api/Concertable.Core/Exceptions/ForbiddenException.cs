using System.Net;

namespace Concertable.Core.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string detail) : base(detail, HttpStatusCode.Forbidden)
    {
        Title = "Forbidden";
    }
}
