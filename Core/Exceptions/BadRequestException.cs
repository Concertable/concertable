using Core.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class BadRequestException : HttpException
    {
        public BadRequestException(string message) : base(message, HttpStatusCode.BadRequest) { }

        public BadRequestException(string message, ErrorType errorType) : base(message, errorType, HttpStatusCode.BadRequest) { }
    }
}
