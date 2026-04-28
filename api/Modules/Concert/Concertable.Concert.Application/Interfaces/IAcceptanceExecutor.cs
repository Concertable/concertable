using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptanceExecutor
{
    Task<AcceptCheckout> CheckoutAsync(int applicationId);
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
}
