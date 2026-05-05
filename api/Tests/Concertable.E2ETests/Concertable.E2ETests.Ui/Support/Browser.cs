using Concertable.E2ETests.Ui.Hooks;
using Microsoft.Extensions.Logging;

namespace Concertable.E2ETests.Ui.Support;

public class Browser : IAsyncDisposable, IDisposable
{
    private readonly ILogger<Browser> logger;
    private IBrowser playwrightBrowser = null!;
    private Role? currentRole;

    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public Browser(ILogger<Browser> logger)
    {
        this.logger = logger;
    }

    public async Task InitializeAsync(IBrowser playwrightBrowser, Role? role)
    {
        this.playwrightBrowser = playwrightBrowser;
        await CreateContextAsync(role);
    }

    public async Task UseRoleAsync(Role role)
    {
        if (currentRole == role) return;
        await SaveTraceAndDisposeAsync();
        await CreateContextAsync(role);
    }

    private async Task CreateContextAsync(Role? role)
    {
        var options = new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
        if (role is not null) options.StorageState = LoginCaptureHooks.GetStorageState(role.Value);
        Context = await playwrightBrowser.NewContextAsync(options);
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = false,
        });
        Page = await Context.NewPageAsync();
        currentRole = role;
    }

    private async Task SaveTraceAndDisposeAsync()
    {
        if (Context is null) return;
        Directory.CreateDirectory("playwright-traces");
        await Context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = $"playwright-traces/trace-{currentRole}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.zip",
        });
        logger.LogInformation("Playwright trace saved to playwright-traces/");
        await Context.DisposeAsync();
        Context = null!;
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        await SaveTraceAndDisposeAsync();
    }
}
