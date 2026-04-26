using System.Net;

namespace Concertable.Shared.Exceptions;

public class InternalServerException : HttpException
{
    public InternalServerException(string detail) : base(detail, HttpStatusCode.InternalServerError)
    {
        Title = "Internal Server Error";
    }
}
