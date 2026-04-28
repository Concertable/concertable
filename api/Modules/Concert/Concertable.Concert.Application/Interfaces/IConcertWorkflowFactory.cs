namespace Concertable.Concert.Application.Interfaces;

internal interface IConcertWorkflowFactory
{
    IConcertWorkflow Create(ContractType type);
}
