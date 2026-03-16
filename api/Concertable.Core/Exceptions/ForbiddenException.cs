using System.Net;

namespace Core.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string detail) : base(detail, HttpStatusCode.Forbidden)
    {
        Title = "Forbidden";
    }
}
