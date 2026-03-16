using Core.Enums;
using System.Net;

namespace Core.Exceptions;

public class BadRequestException : HttpException
{
    public IDictionary<string, string[]>? ValidationErrors { get; }

    public BadRequestException(IDictionary<string, string[]> validationErrors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Title = "Bad Request";
        ValidationErrors = validationErrors;
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
