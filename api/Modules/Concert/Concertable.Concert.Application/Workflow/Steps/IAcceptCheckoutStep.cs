using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Workflow.Steps;

internal interface IAcceptCheckoutStep : IConcertStep
{
    static ConcertStage IConcertStep.Stage => ConcertStage.CheckedOut;
    Task<Checkout> ExecuteAsync(int applicationId);
}
