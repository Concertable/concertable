using System.Net;

namespace Concertable.Core.Exceptions;

public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string detail) : base(detail, HttpStatusCode.Unauthorized)
    {
        Title = "Unauthorized";
    }
}
