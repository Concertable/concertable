namespace Concertable.E2ETests.Ui.Support;

public class Browser : IAsyncDisposable
{
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public async Task InitializeAsync(IBrowser playwrightBrowser, string? storageState)
    {
        var options = new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
        if (storageState is not null) options.StorageState = storageState;
        Context = await playwrightBrowser.NewContextAsync(options);
        Page = await Context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Context is not null) await Context.DisposeAsync();
    }
}
