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
        public IEnumerable<string> Reasons { get; } = new List<string>();

        public BadRequestException(IEnumerable<string> reasons)
            : base("Bad Request", HttpStatusCode.BadRequest) 
        {
            this.Reasons = reasons;
        }

        public BadRequestException(string message)
            : base(message)
        {
        }

        public BadRequestException(string message, ErrorType errorType) : base(message, errorType, HttpStatusCode.BadRequest) { }
    }
}
