using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertWorkflowStrategyFactory(IServiceProvider sp) : IConcertWorkflowStrategyFactory
{
    public IConcertWorkflowStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<IConcertWorkflowStrategy>(type);
}
