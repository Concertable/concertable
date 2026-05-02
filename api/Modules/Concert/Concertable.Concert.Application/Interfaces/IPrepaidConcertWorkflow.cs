namespace Concertable.Concert.Application.Interfaces;

internal interface IPrepaidConcertWorkflow
    : IConcertWorkflow, IPaidApply, ISimpleAccept
{
}
