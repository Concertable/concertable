using System.Net;

namespace Concertable.Application.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string detail) : base(detail, HttpStatusCode.Forbidden)
    {
        Title = "Forbidden";
    }
}
