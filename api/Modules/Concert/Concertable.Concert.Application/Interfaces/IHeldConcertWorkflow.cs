namespace Concertable.Concert.Application.Interfaces;

internal interface IHeldConcertWorkflow
    : IConcertWorkflow, ISimpleApply, IAcceptCheckout, ISimpleAccept
{
}
