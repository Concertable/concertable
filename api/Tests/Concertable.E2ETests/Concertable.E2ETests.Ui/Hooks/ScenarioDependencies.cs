using Concertable.E2ETests.Ui.Support;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace Concertable.E2ETests.Ui.Hooks;

public static class ScenarioDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_ => TestRunHooks.Fixture);
        services.AddScoped<Browser>();
        services.AddScoped<WorkflowState>();
        return services;
    }
}
