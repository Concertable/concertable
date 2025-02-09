using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class PaymentRequiredException : HttpException
    {
        public PaymentRequiredException(string message, HttpStatusCode statusCode = HttpStatusCode.PaymentRequired) : base(message, statusCode)
        {
        }
    }
}
