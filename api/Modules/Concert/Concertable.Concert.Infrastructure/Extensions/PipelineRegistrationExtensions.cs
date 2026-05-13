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
        configure(new ConcertPipelineBuilder(contractType, services));
        return services;
    }
}
