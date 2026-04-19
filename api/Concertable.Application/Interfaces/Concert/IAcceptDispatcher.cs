using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

public interface IAcceptDispatcher
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
}
