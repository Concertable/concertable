namespace Concertable.Concert.Application.Interfaces;

internal interface IAuthAccept : IAcceptable
{
    Task AcceptAsync(int applicationId, string paymentMethodId);
}
