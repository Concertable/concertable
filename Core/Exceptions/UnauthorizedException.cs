using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions;

public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string message) : base(message, HttpStatusCode.Unauthorized) { }
}
