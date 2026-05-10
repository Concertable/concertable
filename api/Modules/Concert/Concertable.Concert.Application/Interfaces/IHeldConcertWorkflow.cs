namespace Concertable.Concert.Application.Interfaces;

internal interface IDirectConcertWorkflow
    : IConcertWorkflow, ISimpleApply, IAcceptCheckout, ISimpleAccept
{
}
