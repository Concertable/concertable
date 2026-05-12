using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IApplyCheckoutStep : IConcertStep
{
    Task<Checkout> ExecuteAsync(int opportunityId);
}
