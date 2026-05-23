using System.Net;

namespace Concertable.Kernel.Exceptions;

public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string detail) : base(detail, HttpStatusCode.Unauthorized)
    {
        Title = "Unauthorized";
    }
}
