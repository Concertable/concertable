namespace Concertable.Concert.Application.Interfaces;

internal interface ISimpleAccept : IAcceptable
{
    Task AcceptAsync(int applicationId);
}
