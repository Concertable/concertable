using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptByConfirmation : IAcceptable
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId);
}
