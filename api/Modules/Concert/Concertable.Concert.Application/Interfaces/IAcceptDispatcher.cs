using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptDispatcher
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
}
