using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertWorkflowFactory(IServiceProvider sp) : IConcertWorkflowFactory
{
    public IConcertWorkflow Create(ContractType type)
        => sp.GetRequiredKeyedService<IConcertWorkflow>(type);
}
