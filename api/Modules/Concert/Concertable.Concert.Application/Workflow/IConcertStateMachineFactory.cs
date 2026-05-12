namespace Concertable.Concert.Application.Workflow;

internal interface IConcertStateMachineFactory
{
    IConcertStateMachine Create(ContractType contractType);
}
