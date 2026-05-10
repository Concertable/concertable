namespace Concertable.Concert.Application.Interfaces;

internal interface IDeferredConcertWorkflow
    : IConcertWorkflow, ISimpleApply, IAcceptCheckout, IPaidAccept
{
}
