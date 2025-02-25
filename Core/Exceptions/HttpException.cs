using Core.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public ErrorType? ErrorType { get; }

        public HttpException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public HttpException(string message, ErrorType errorType, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
        {
            this.StatusCode = statusCode;
            this.ErrorType = errorType;
        }


    }
}
