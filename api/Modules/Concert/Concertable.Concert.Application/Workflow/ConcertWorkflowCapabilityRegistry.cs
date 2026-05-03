using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Workflow;

internal sealed class ConcertWorkflowCapabilityRegistry
{
    private readonly IReadOnlyDictionary<ContractType, Type> strategyTypes;

    public ConcertWorkflowCapabilityRegistry(IReadOnlyDictionary<ContractType, Type> strategyTypes)
        => this.strategyTypes = strategyTypes;

    public bool Has<TStep>(ContractType ct) where TStep : IConcertWorkflowStep
        => strategyTypes[ct].IsAssignableTo(typeof(TStep));
}
