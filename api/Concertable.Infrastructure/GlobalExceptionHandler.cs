using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Concertable.Core.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Diagnostics;

namespace Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger;
    private readonly IHostEnvironment env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        this.logger = logger;
        this.env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is HttpException httpEx)
        {
            httpContext.Response.StatusCode = (int)httpEx.StatusCode;
            problemDetails.Status = httpContext.Response.StatusCode;
            problemDetails.Title = httpEx.Title;
            problemDetails.Detail = httpEx.Detail;

            if (exception is BadRequestException badRequestEx && badRequestEx.ValidationErrors is not null)
            {
                problemDetails.Detail = httpEx.Message;
                problemDetails.Extensions["errors"] = badRequestEx.ValidationErrors;
            }
            else
            {
                problemDetails.Detail = httpEx.Message;
            }
        }
        else
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            problemDetails.Status = httpContext.Response.StatusCode;
            problemDetails.Title = "Internal Server Error";
            problemDetails.Detail = exception.Message;

            problemDetails.Extensions["stackTrace"] = exception.ToString();

            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.ToString();
            }
        }

        logger.LogError(exception, "Unhandled exception occurred");

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
