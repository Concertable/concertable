using System.Net;

namespace Concertable.Application.Exceptions;

public class InternalServerException : HttpException
{
    public InternalServerException(string detail) : base(detail, HttpStatusCode.InternalServerError)
    {
        Title = "Internal Server Error";
    }
}
