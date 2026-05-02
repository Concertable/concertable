using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IAcceptCheckout
{
    Task<AcceptCheckout> CheckoutAsync(int applicationId);
}
