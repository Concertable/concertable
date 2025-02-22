using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class ForbiddenException : HttpException
    {
        public ForbiddenException(string message) : base(message, HttpStatusCode.Forbidden) { }
    }
}
