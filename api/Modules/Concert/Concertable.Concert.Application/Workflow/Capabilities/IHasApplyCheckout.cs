using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Application.Workflow.Capabilities;

internal interface IHasApplyCheckout
{
    IApplyCheckoutStep ApplyCheckout { get; }
}
