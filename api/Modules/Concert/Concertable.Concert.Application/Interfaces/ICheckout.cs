using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface ICheckout : ICheckoutable
{
    Task<AcceptCheckout> CheckoutAsync(int applicationId);
}
