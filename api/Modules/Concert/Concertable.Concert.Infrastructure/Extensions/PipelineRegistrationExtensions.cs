using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Infrastructure.Services.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Extensions;

internal static class PipelineRegistrationExtensions
{
    public static IServiceCollection AddConcertPipeline(
        this IServiceCollection services,
        ContractType contractType,
        Action<ConcertPipelineBuilder> configure)
    {
        var registry = services.GetOrAddSingleton(new ConcertPipelineRegistry());
        var builder = new ConcertPipelineBuilder(contractType, services, registry);
        configure(builder);
        return services;
    }

    private static ConcertPipelineRegistry GetOrAddSingleton(this IServiceCollection services, ConcertPipelineRegistry instance)
    {
        var existing = services.FirstOrDefault(d => d.ServiceType == typeof(ConcertPipelineRegistry))?.ImplementationInstance as ConcertPipelineRegistry;
        if (existing is not null)
            return existing;

        services.AddSingleton(instance);
        services.AddSingleton<IConcertPipelineRegistry>(instance);
        return instance;
    }
}
