using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IPaidAccept : IAcceptable
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId);
}
