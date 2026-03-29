using Concertable.Core.Enums;
using System.Net;

namespace Concertable.Core.Exceptions;

public class BadRequestException : HttpException
{
    public IReadOnlyList<string>? ValidationErrors { get; }

    public BadRequestException(IReadOnlyList<string> errors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Title = "Bad Request";
        ValidationErrors = errors;
    }

    public BadRequestException(string detail)
        : base(detail, HttpStatusCode.BadRequest)
    {
        Title = "Bad Request";
    }

    public BadRequestException(string detail, ErrorType errorType)
        : base(detail, errorType, HttpStatusCode.BadRequest)
    {
        Title = "Bad Request";
    }
}
