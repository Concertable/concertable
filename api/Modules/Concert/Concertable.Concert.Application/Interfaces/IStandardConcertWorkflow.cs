namespace Concertable.Concert.Application.Interfaces;

internal interface IStandardConcertWorkflow
    : IConcertWorkflow, ISimpleApply, ICheckout, IPaidAccept
{
}
