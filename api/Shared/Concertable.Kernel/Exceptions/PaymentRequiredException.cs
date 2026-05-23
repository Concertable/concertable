using System.Net;

namespace Concertable.Kernel.Exceptions;

public class PaymentRequiredException : HttpException
{
    public PaymentRequiredException(string detail) : base(detail, HttpStatusCode.PaymentRequired)
    {
        Title = "Payment Required";
    }
}
