using Concertable.Core.Enums;
using System.Net;

namespace Concertable.Application.Exceptions;

public class HttpException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ErrorType? ErrorType { get; }
    public string Title { get; protected set; } = "An error occurred";
    public string Detail { get; }

    public HttpException(string detail, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(detail)
    {
        StatusCode = statusCode;
        Detail = detail;
    }

    public HttpException(string detail, ErrorType errorType, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(detail)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
        Detail = detail;
    }
}
