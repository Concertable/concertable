using Concertable.E2ETests.Ui.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace Concertable.E2ETests.Ui.Hooks;

public static class ScenarioDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddSingleton(_ => PlaywrightHooks.Fixture);
        services.AddScoped<Browser>();
        services.AddScoped<WorkflowState>();
        return services;
    }
}
