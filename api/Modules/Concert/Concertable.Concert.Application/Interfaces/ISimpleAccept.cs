using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface ISimpleAccept : IAcceptable
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId);
}
