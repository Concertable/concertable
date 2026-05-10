namespace Concertable.Concert.Application.Interfaces;

internal interface IPaidAccept : IAcceptable
{
    Task AcceptAsync(int applicationId, string paymentMethodId);
}
