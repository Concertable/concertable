using System.Net;

namespace Concertable.Shared.Exceptions;

public class PaymentRequiredException : HttpException
{
    public PaymentRequiredException(string detail) : base(detail, HttpStatusCode.PaymentRequired)
    {
        Title = "Payment Required";
    }
}
