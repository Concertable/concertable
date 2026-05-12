using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IAcceptCheckoutStep : IConcertStep
{
    Task<Checkout> ExecuteAsync(int applicationId);
}
