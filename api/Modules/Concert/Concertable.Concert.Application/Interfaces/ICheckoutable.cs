using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface ICheckoutable : IConcertWorkflowStep
{
    Task<Checkout> CheckoutAsync(int contextId);
}
