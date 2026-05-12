using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertPipelineRegistry : IConcertPipelineRegistry
{
    private readonly Dictionary<ContractType, HashSet<Type>> stepTypes = [];

    public void Register(ContractType contractType, Type stepInterface)
    {
        if (!stepTypes.TryGetValue(contractType, out var set))
        {
            set = [];
            stepTypes[contractType] = set;
        }
        set.Add(stepInterface);
    }

    public bool Has<TStep>(ContractType contractType) where TStep : IConcertStep =>
        stepTypes.TryGetValue(contractType, out var set) && set.Contains(typeof(TStep));
}
