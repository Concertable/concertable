using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptanceDispatcher
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId);
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId);
}
