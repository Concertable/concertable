namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertWorkflowStrategyFactory
{
    IConcertWorkflowStrategy Create(ContractType type);
}
