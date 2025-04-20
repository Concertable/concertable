using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;
using Stripe.Issuing;

namespace Infrastructure
{
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

            // Handle BadRequestException or other HttpException (inherits from HttpException)
            if (exception is HttpException httpEx)
            {
                // Set the status code from the exception (this covers both HttpException and BadRequestException)
                httpContext.Response.StatusCode = (int)httpEx.StatusCode;
                problemDetails.Title = httpEx.Message;

                // If it's a BadRequestException, handle validation errors
                if (exception is BadRequestException badRequestEx)
                {
                    if (badRequestEx.Reasons.Any())
                    {
                        problemDetails.Title = "Validation Error";
                        problemDetails.Detail = "One or more validation errors occurred.";
                        problemDetails.Extensions["errors"] = badRequestEx.Reasons; // Attach validation errors
                    }
                }
                else
                {
                    // For other HttpExceptions, just use the message as detail
                    problemDetails.Detail = httpEx.Message;
                }
            }
            else
            {
                // Default error handling for non-HttpException cases
                problemDetails.Title = exception.Message;
                problemDetails.Detail = exception.StackTrace;
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            // Log the error
            logger.LogError("{ProblemDetailsTitle}", problemDetails.Title);
            problemDetails.Status = httpContext.Response.StatusCode;

            // If in development, show the full stack trace
            if (env.IsDevelopment())
            {
                problemDetails.Detail = exception.StackTrace;
                logger.LogError("Stack Trace", problemDetails.Detail);
            }

            // Return error response to the client
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
