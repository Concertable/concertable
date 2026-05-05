using Concertable.E2ETests.Ui.Hooks;

namespace Concertable.E2ETests.Ui.Support;

public class Browser : IAsyncDisposable, IDisposable
{
    private IBrowser playwrightBrowser = null!;
    private Role? currentRole;

    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync(IBrowser playwrightBrowser, Role? role)
    {
        this.playwrightBrowser = playwrightBrowser;
        await CreateContextAsync(role);
    }

    public async Task UseRoleAsync(Role role)
    {
        if (currentRole == role) return;
        await Context.DisposeAsync();
        await CreateContextAsync(role);
    }

    private async Task CreateContextAsync(Role? role)
    {
        var options = new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
        if (role is not null) options.StorageState = LoginCaptureHooks.GetStorageState(role.Value);
        Context = await playwrightBrowser.NewContextAsync(options);
        Page = await Context.NewPageAsync();
        currentRole = role;
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        if (Context is not null) await Context.DisposeAsync();
    }
}
