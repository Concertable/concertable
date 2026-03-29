using System.Net;

namespace Concertable.Core.Exceptions;

public class NotFoundException : HttpException
{
    public NotFoundException(string detail) : base(detail, HttpStatusCode.NotFound)
    {
        Title = "Not Found";
    }
}
