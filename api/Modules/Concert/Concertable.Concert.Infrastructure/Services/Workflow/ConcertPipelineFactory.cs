using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertPipelineFactory : IConcertPipelineFactory
{
    private readonly IKeyedServiceProvider serviceProvider;
    private readonly IConcertPipelineRegistry registry;

    public ConcertPipelineFactory(IKeyedServiceProvider serviceProvider, IConcertPipelineRegistry registry)
    {
        this.serviceProvider = serviceProvider;
        this.registry = registry;
    }

    public TStep? Create<TStep>(ContractType contractType) where TStep : class, IConcertStep =>
        registry.Has<TStep>(contractType)
            ? serviceProvider.GetRequiredKeyedService<TStep>(contractType)
            : null;
}
