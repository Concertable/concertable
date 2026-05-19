using System.Net;

namespace Concertable.Shared.Exceptions;

public class ConflictException : HttpException
{
    public ConflictException(string detail)
        : base(detail, HttpStatusCode.Conflict)
    {
        Title = "Conflict";
    }
}
