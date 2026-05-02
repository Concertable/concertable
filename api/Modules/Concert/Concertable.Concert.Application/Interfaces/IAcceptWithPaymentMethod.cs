using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptWithPaymentMethod
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId);
}
