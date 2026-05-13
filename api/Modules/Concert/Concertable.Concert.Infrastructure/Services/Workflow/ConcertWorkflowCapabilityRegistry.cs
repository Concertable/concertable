using Concertable.Concert.Application.Workflow;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertWorkflowCapabilityRegistry : IConcertWorkflowCapabilityRegistry
{
    private readonly IConcertWorkflowFactory workflows;

    public ConcertWorkflowCapabilityRegistry(IConcertWorkflowFactory workflows)
        => this.workflows = workflows;

    public bool Has<TCapability>(ContractType contractType) where TCapability : class
        => workflows.Create(contractType) is TCapability;
}
