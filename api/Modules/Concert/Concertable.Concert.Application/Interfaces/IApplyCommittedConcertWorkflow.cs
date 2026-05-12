namespace Concertable.Concert.Application.Interfaces;

internal interface IApplyCommittedConcertWorkflow
    : IConcertWorkflow, IPaidApply, IApplyCheckout, ISimpleAccept
{
}
